using System;
using System.IO;
using Akka.Configuration;
using Autofac;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Boottrap
{
    public static class Boottrap
    {
        public static IApplicationBuilder StartNode(this IApplicationBuilder builder)
        {
            return builder
               .ConfigureAutoFac(cb => cb.RegisterType<EmptyAppRoute>().Named<IAppRoute>("default"))
               .ConfigureAkka(hbc =>
                              {
                                  Console.Title = hbc.HostEnvironment.ApplicationName;

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
                                     configuration.WriteTo.RollingFile(new CompactJsonFormatter(), "Logs\\Log.log", fileSizeLimitBytes: 5_242_880, retainedFileCountLimit: 5);
                                     configuration.WriteTo.ColoredConsole();

                                     configuration
                                        .MinimumLevel.Debug()
                                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                                        .Enrich.WithProperty("ApplicationName", context.HostEnvironment.ApplicationName)
                                        .Enrich.FromLogContext()
                                        .Enrich.WithExceptionDetails()
                                        .Enrich.With<EventTypeEnricher>();
                                 })
               .ConfigurateAkkSystem((context, system) => KillSwitch.Enable(system));
        }
    }
}