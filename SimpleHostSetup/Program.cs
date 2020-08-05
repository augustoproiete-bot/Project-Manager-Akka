using System;
using System.Threading.Tasks;
using Serilog;
using SimpleHostSetup.Impl;
using Tauron.Application.Logging;

namespace SimpleHostSetup
{

    //InstallSeed.bat
    //cd %~dp0\TestHost
    //ServiceHost.exe --Install Manual --ZipFile..\Seed.zip --AppName Master-Seed --AppType StartUp
    //pause
    //
    //StartHost.bat
    //cd %~dp0\TestHost
    //ServiceHost.exe
    //pause




    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Title = "Simple Setup Builder";

            Log.Logger = new LoggerConfiguration().ConfigDefaultLogging("Simple Setup Builder", true).WriteTo.ColoredConsole().CreateLogger();

            var system = new BuildSystem();
            await system.Run();
        }
    }
}
