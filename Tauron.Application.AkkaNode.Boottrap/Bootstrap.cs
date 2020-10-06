using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Tauron.Application.AkkNode.Services.Configuration;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Bootstrap
{
    public static class Bootstrap
    {
        public static IApplicationBuilder StartNode(string[] args, KillRecpientType type)
        {

            //Assemblys.WireUp();
            return ActorApplication.Create(args)
               .ConfigureAutoFac(cb =>
                {
                    cb.RegisterType<ConsoleAppRoute>().Named<IAppRoute>("default");
                    cb.RegisterType<KillHelper>().As<IStartUpAction>();
                })
               .ConfigurateNode()
               .ConfigureLogging((context, configuration) =>
                {
                    Console.Title = context.HostEnvironment.ApplicationName;

                    configuration.WriteTo.ColoredConsole();
                })
               .ConfigurateAkkaSystem((context, system) =>
                {
                    if (type == KillRecpientType.Seed)
                        KillSwitch.Setup(system);
                    else
                        KillSwitch.Subscribe(system, type);
                });
        }

        public static IApplicationBuilder ConfigurateNode(this IApplicationBuilder builder)
        {
            var config = GetConfig();

            return builder
               .ConfigureAppConfiguration((context, configurationBuilder) => configurationBuilder.Add(
                    new HoconConfigurationSource(() => config, 
                        ("akka.appinfo.applicationName", "applicationName"),
                        ("akka.appinfo.actorsystem", "actorsystem"),
                        ("akka.appinfo.appslocation", "AppsLocation"))))
               .ConfigureAkka(hbc => config)
               .ConfigureLogging((context, configuration) =>
                {
                    configuration.WriteTo.RollingFile(new CompactJsonFormatter(), "Logs\\Log.log", fileSizeLimitBytes: 5_242_880, retainedFileCountLimit: 2);

                    configuration
                       .MinimumLevel.Debug()
                       .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                       .MinimumLevel.Override("System", LogEventLevel.Warning)
                       .Enrich.WithProperty("ApplicationName", context.HostEnvironment.ApplicationName)
                       .Enrich.FromLogContext()
                       .Enrich.WithExceptionDetails()
                       .Enrich.With<EventTypeEnricher>()
                       .Enrich.With<LogLevelWriter>();
                });
        }

        private static Config GetConfig()
        {
            const string baseconf = "base.conf";
            const string main = "akka.conf";
            const string seed = "seed.conf";

            var config = Config.Empty;

            if (File.Exists(baseconf))
                config = ConfigurationFactory.ParseString(File.ReadAllText(baseconf)).WithFallback(config);

            if (File.Exists(main))
                config = ConfigurationFactory.ParseString(File.ReadAllText(main)).WithFallback(config);

            if (File.Exists(seed))
                config = ConfigurationFactory.ParseString(File.ReadAllText(seed)).WithFallback(config);

            return config;
        }

        private class LogLevelWriter : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Level", new ScalarValue(logEvent.Level)));
            }
        }

        [UsedImplicitly]
        private sealed class KillHelper : IStartUpAction
        {
            [UsedImplicitly]
            private static KillHelper? _keeper;

            private readonly ActorSystem _system;
            private readonly ILogger _logger;
            private readonly string? _comHandle;

            public KillHelper(IConfiguration configuration, ActorSystem system)
            {
                _logger = Log.ForContext<KillHelper>();
                _comHandle = configuration["ComHandle"];
                _system = system;

                _keeper = this;
                _system.RegisterOnTermination(() => _keeper = null);
            }

            public void Run()
            {
                Task.Factory.StartNew(() =>
                {
                    if (_keeper == null || string.IsNullOrWhiteSpace(_comHandle))
                        return;

                    try
                    {
                        using var client = new AnonymousPipeClientStream(PipeDirection.In, _comHandle);
                        using var reader = new BinaryReader(client);

                        while (_keeper != null)
                        {
                            var data = reader.ReadString();

                            switch (data)
                            {
                                case "Kill-Node":
                                    _logger.Information("Reciving Killing Notification");
                                    _system.Terminate();
                                    _keeper = null;
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error on Setup Service kill Watch");
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }
    }
}