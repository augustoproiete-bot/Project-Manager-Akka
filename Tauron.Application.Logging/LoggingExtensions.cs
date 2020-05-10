using System;
using Autofac;
using JetBrains.Annotations;
using Serilog;
using Serilog.Configuration;
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
                loggerConfiguration.WriteTo.RollingFile(new CompactJsonFormatter(), "Logs\\Log.log", fileSizeLimitBytes: 5_242_880, retainedFileCountLimit:5);

            return loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithEventTypeEnricher();
        }

        public static LoggerConfiguration WithEventTypeEnricher(this LoggerEnrichmentConfiguration config)
            => config.With<EventTypeEnricher>();

        public static ContainerBuilder AddTauronLogging(this ContainerBuilder collection)
        {
            collection.RegisterGeneric(typeof(SeriLogger<>)).As(typeof(ISLogger<>));

            return collection;
        }
    }
}