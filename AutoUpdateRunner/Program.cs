using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AutoUpdateRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Host Updater";
            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().Enrich.FromLogContext().CreateLogger();

            var config = new ConfigurationBuilder().AddCommandLine(args).Build();
            var setup = new SetupInfo();
            config.Bind(setup);

            new SetupRunner(setup).Run();
        }
    }
}
