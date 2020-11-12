using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace AutoUpdateRunner
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Host Updater";
            Log.Logger = new LoggerConfiguration().WriteTo.Console(theme:AnsiConsoleTheme.Literate).Enrich.FromLogContext().CreateLogger();

            var config = new ConfigurationBuilder().AddCommandLine(args).Build();
            var setup = new SetupInfo();
            config.Bind(setup);

            new SetupRunner(setup).Run();
        }
    }
}
