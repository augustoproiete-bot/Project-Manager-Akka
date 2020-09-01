using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using SimpleHostSetup.DotnetBuilder;
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




    public sealed class Program : IInput
    {
        private readonly Dictionary<string, string> _presets = new Dictionary<string, string>();
        private readonly IConfiguration _commandLine;

        public static async Task Main(string[] args)
        {
            Console.Title = "Simple Setup Builder";

            Log.Logger = new LoggerConfiguration().ConfigDefaultLogging("Simple Setup Builder", true).WriteTo.ColoredConsole().CreateLogger();

            string searchStart = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.CodeBase) ?? string.Empty);
            var config = new BuildSystemConfiguration(searchStart, "Project-Manager-Akka.sln", new Program(args), () => new DotNetBuilder(), 
                ImmutableDictionary<string, AppInfo>.Empty.Add("Seed", new AppInfo(AppType.StartUp, "Master.Seed.Node.csproj")), "ServiceHost.csproj");

            var system = new BuildSystem();
            await system.Run(config);
        }

        private Program(string[] args)
        {
            foreach (var t in JToken.Parse(File.ReadAllText("preset.json")).Cast<JProperty>()) 
                _presets[t.Name] = t.Value.Value<string>();

            _commandLine = new ConfigurationBuilder().AddCommandLine(args).Build();
        }

        private string FindValue(string consoleLabel, string commandLine, string? errorText = null)
        {
            Console.Write(consoleLabel);

            string value = _commandLine[commandLine];
            if (string.IsNullOrWhiteSpace(value)) 
                value = Console.ReadLine() ?? throw new InvalidOperationException(errorText ?? $"It was no {commandLine} Read");
            else
                Console.WriteLine(value);

            return _presets.TryGetValue(value, out var replacer) ? replacer : value;
        }

        public string GetIp() => FindValue("Server Ip: ", "ip");

        public string GetSeed() => FindValue("Seed Ip: ", "seed");

        public string GetName(string @for) => FindValue($"Application Name {@for}: ", @for + "-app", $"No Application Name for {@for}");

        public IEnumerable<string> GetAppsToInstall(IEnumerable<string> apps) => apps;
    }
}
