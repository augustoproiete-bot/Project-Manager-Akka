using System;
using System.IO;
using Akka.Configuration;
using Autofac;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Boottrap
{
    public static class Boottrap
    {
        public static IApplicationBuilder StartNode(this IApplicationBuilder builder, KillRecpientType type)
        {
            return builder
               .ConfigureAutoFac(cb => cb.RegisterType<EmptyAppRoute>().Named<IAppRoute>("default"))
               .ConfigurateNode()
               .ConfigureLogging((context, configuration) =>
                                 {
                                     Console.Title = context.HostEnvironment.ApplicationName;

                                     configuration.WriteTo.ColoredConsole();
                                 })
               .ConfigurateAkkaSystem((context, system) =>
               {
                   if(type == KillRecpientType.Seed)
                       KillSwitch.Setup(system);
                   else
                       KillSwitch.Subscribe(system, type);
               });
        }

        public static IApplicationBuilder ConfigurateNode(this IApplicationBuilder builder)
        {
            return builder
               .ConfigureAkka(hbc =>
                              {
                                  const string main = "akka.conf";
                                  const string seed = "seed.conf";

                                  var config = ConfigurationFactory.Empty;

                                  if (File.Exists(main))
                                      config = ConfigurationFactory.ParseString(File.ReadAllText(main)).WithFallback(config);

                                  if (File.Exists(seed))
                                      config = ConfigurationFactory.ParseString(File.ReadAllText(seed)).WithFallback(config);

                                  return config;
                              })
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

        public class LogLevelWriter : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Level", new ScalarValue(logEvent.Level)));
            }
        }
    }
}