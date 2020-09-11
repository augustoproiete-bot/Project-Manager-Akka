using System.Diagnostics;
using Autofac;
using JetBrains.Annotations;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Tauron.Application.Logging.impl;

namespace Tauron.Application.Logging
{
    [PublicAPI]
    public static class LoggingExtensions
    {
        public static LoggerConfiguration ConfigDefaultLogging(this LoggerConfiguration loggerConfiguration, string applicationName, bool noFile = false)
        {
            if (!noFile)
                loggerConfiguration.WriteTo.RollingFile(new CompactJsonFormatter(), "Logs\\Log.log", fileSizeLimitBytes: 5_242_880, retainedFileCountLimit: 5);

            return loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.With<LogLevelWriter>()
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithEventTypeEnricher();
        }

        public static LoggerConfiguration WithEventTypeEnricher(this LoggerEnrichmentConfiguration config)
        {
            return config.With<EventTypeEnricher>();
        }

        public static ContainerBuilder AddTauronLogging(this ContainerBuilder collection)
        {
            collection.RegisterGeneric(typeof(SeriLogger<>)).As(typeof(ISLogger<>));

            return collection;
        }
    }

    public class LogLevelWriter : ILogEventEnricher
    {
        [DebuggerHidden]
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty("Level", new ScalarValue(logEvent.Level)));
        }
    }
}