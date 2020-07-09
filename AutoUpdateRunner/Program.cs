using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AutoUpdateRunner
{
    class Program
    {
        static async Task Main(string[] args)
        { 
            Log.Logger = new LoggerConfiguration().WriteTo.ColoredConsole().Enrich.FromLogContext().CreateLogger();

            var config = new ConfigurationBuilder().AddCommandLine(args).Build();
            var setup = new SetupInfo();
            config.Bind(setup);

            await new SetupRunner(setup).Run();
        }
    }
}
