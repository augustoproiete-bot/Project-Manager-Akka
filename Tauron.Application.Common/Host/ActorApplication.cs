using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Tauron.Host
{
    [PublicAPI]
    public sealed class ActorApplication : IDisposable
    {
        private static ActorApplication? _actorApplication;

        internal ActorApplication(IContainer continer, ActorSystem actorSystem)
        {
            _actorApplication = this;
            Continer = continer;
            ActorSystem = actorSystem;
        }

        public static ActorApplication Application => Argument.NotNull(_actorApplication, nameof(Application));

        public IContainer Continer { get; }
        public ActorSystem ActorSystem { get; }

        public void Dispose()
        {
            Continer.Dispose();
        }

        public static IApplicationBuilder Create(string[]? args = null)
        {
            var builder = new Builder();
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder
                .ConfigureAkka(he => ConfigurationFactory.ParseString(" akka { loggers =[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"] \n  scheduler { implementation = \"Tauron.Akka.TimerScheduler, Tauron.Application.Common\" } }"))
                .ConfigureAutoFac(cb => cb.RegisterModule<CommonModule>())
                .Configuration(cb =>
                {
                    cb.AddEnvironmentVariables("DOTNET_");
                    if (args != null)
                        cb.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    IHostEnvironment hostEnvironment = hostingContext.HostEnvironment;
                    var value = hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", true);
                    config.AddJsonFile("appsettings." + hostEnvironment.EnvironmentName + ".json", true, value);
                    config.AddEnvironmentVariables();
                })
               .Configuration(cb => cb.AddJsonFile("appsettings.json", true, true));

            return builder;
        }

        public async Task Run()
        {
            var lifeTime = Continer.Resolve<IHostLifetime>();
            var hostAppLifetime = (ApplicationLifetime) Continer.Resolve<IHostApplicationLifetime>();
            await using (hostAppLifetime.ApplicationStopping.Register(() => ActorSystem.Terminate()))
            {
                await lifeTime.WaitForStartAsync(ActorSystem);
                hostAppLifetime.NotifyStarted();

                ActorSystem.RegisterOnTermination(hostAppLifetime.NotifyStopped);
                await lifeTime.ShutdownTask;
                await Task.WhenAny(ActorSystem.WhenTerminated, Task.Delay(TimeSpan.FromSeconds(60)));
            }
        }

        private sealed class Builder : IApplicationBuilder
        {
            private readonly List<Action<HostBuilderContext, ActorSystem>> _actorSystemConfig = new List<Action<HostBuilderContext, ActorSystem>>();
            private readonly List<Func<HostBuilderContext, Config>> _akkaConfig = new List<Func<HostBuilderContext, Config>>();
            private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _appConfigs = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
            private readonly List<Action<IConfigurationBuilder>> _configurationBuilders = new List<Action<IConfigurationBuilder>>();
            private readonly List<Action<ContainerBuilder>> _containerBuilder = new List<Action<ContainerBuilder>>();
            private readonly List<Action<HostBuilderContext, LoggerConfiguration>> _logger = new List<Action<HostBuilderContext, LoggerConfiguration>>();

            public IApplicationBuilder ConfigureLogging(Action<HostBuilderContext, LoggerConfiguration> config)
            {
                _logger.Add(config);
                return this;
            }

            public IApplicationBuilder Configuration(Action<IConfigurationBuilder> config)
            {
                _configurationBuilders.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config)
            {
                _containerBuilder.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> config)
            {
                _appConfigs.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config)
            {
                _akkaConfig.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigurateAkkSystem(Action<HostBuilderContext, ActorSystem> system)
            {
                _actorSystemConfig.Add(system);
                return this;
            }

            public ActorApplication Build()
            {
                var config = CreateHostConfiguration();
                var hostingEnwiroment = CreateHostingEnvironment(config);
                var context = CreateHostBuilderContext(hostingEnwiroment, config);
                ConfigureLogging(context);
                config = BuildAppConfiguration(hostingEnwiroment, config, context);
                context.Configuration = config;
                var akkaConfig = CreateAkkaConfig(context);
                var system = ActorSystem.Create(GetActorSystemName(context.Configuration, context.HostEnvironment), akkaConfig);

                var continer = CreateServiceProvider(hostingEnwiroment, context, config, system);

                system.AddDependencyResolver(new AutoFacDependencyResolver(continer, system));
                foreach (var action in _actorSystemConfig)
                    action(context, system);

                return new ActorApplication(continer, system);
            }

            private string GetActorSystemName(IConfiguration config, IHostEnvironment environment)
            {
                var name = config["actorsystem"];
                return !string.IsNullOrWhiteSpace(name) 
                    ? name 
                    : environment.ApplicationName.Replace('.', '-');
            }

            private void ConfigureLogging(HostBuilderContext context)
            {
                var config = new LoggerConfiguration();

                foreach (var action in _logger)
                    action(context, config);

                Log.Logger = config.CreateLogger();
            }

            private IConfiguration CreateHostConfiguration()
            {
                var builder = new ConfigurationBuilder().AddInMemoryCollection();
                foreach (var action in _configurationBuilders)
                    action(builder);

                return builder.Build();
            }

            private IHostEnvironment CreateHostingEnvironment(IConfiguration hostConfiguration)
            {
                var hostingEnvironment = new HostEnviroment
                {
                    ApplicationName = hostConfiguration[HostDefaults.ApplicationKey],
                    EnvironmentName = hostConfiguration[HostDefaults.EnvironmentKey] ?? Environments.Production,
                    ContentRootPath = ResolveContentRootPath(hostConfiguration[HostDefaults.ContentRootKey], AppContext.BaseDirectory)
                };
                if (string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
                    hostingEnvironment.ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

                return hostingEnvironment;
            }

            private IConfiguration BuildAppConfiguration(IHostEnvironment hostEnvironment, IConfiguration hostConfiguration, HostBuilderContext hostBuilderContext)
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(hostEnvironment.ContentRootPath)
                    .AddConfiguration(hostConfiguration, true);
                foreach (Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigAction in _appConfigs)
                    configureAppConfigAction(hostBuilderContext, configurationBuilder);
                return configurationBuilder.Build();
            }

            private HostBuilderContext CreateHostBuilderContext(IHostEnvironment environment, IConfiguration configuration)
            {
                return new HostBuilderContext(new Dictionary<object, object>(), configuration, environment);
            }

            private Config CreateAkkaConfig(HostBuilderContext context)
            {
                return _akkaConfig.Aggregate(Config.Empty, (current, func) => current.WithFallback(func(context)));
            }

            private IContainer CreateServiceProvider(IHostEnvironment hostEnvironment, HostBuilderContext hostBuilderContext, IConfiguration appConfiguration, ActorSystem actorSystem)
            {
                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterInstance(actorSystem);
                containerBuilder.RegisterInstance(hostEnvironment);
                containerBuilder.RegisterInstance(hostBuilderContext);
                containerBuilder.RegisterInstance(appConfiguration);
                containerBuilder.RegisterType<ApplicationLifetime>().As<IHostApplicationLifetime, IApplicationLifetime>().SingleInstance();
                containerBuilder.RegisterType<CommonLifetime>().As<IHostLifetime>().SingleInstance();

                foreach (var action in _containerBuilder)
                    action(containerBuilder);

                return containerBuilder.Build();
            }

            private static string ResolveContentRootPath(string contentRootPath, string basePath)
            {
                if (string.IsNullOrEmpty(contentRootPath)) return basePath;
                return Path.IsPathRooted(contentRootPath) ? contentRootPath : Path.Combine(Path.GetFullPath(basePath), contentRootPath);
            }
        }
    }
}