using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Serilog;
using Serilog.Parsing;
using ServiceManager.ProjectRepository;
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.ServiceManager.Core.Configuration;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class SetupBuilder
    {
        public static readonly string BuildRoot = Path.Combine(Env.Path, "Build");


        private static readonly ILogger Logger = Log.ForContext<SetupBuilder>();
        private readonly RunContext _context;

        private readonly AppConfig _config;
        private Action<string> _log;

        private void LogMessage(string template, params object[] args)
        {
            Logger.Information(template, args);

            var parser = new MessageTemplateParser();
            var template2 = parser.Parse(template);
            var format = new StringBuilder();
            var index = 0;
            foreach (var tok in template2.Tokens)
            {
                if (tok is TextToken)
                    format.Append(tok);
                else
                    format.Append("{" + index++ + "}");
            }
            var netStyle = format.ToString();

            _log(string.Format(netStyle, args));
        }

        public SetupBuilder(string hostName, string? seedHostName, AppConfig config, Action<string> log)
        {
            _context = new RunContext(new RepositoryConfiguration(LogMessage))
            {
                HostName = hostName,
                SeedHostName = seedHostName
            };
            _config = config;
            _log = log;
        }

        public BuildResult? Run(Action<string> log, string identifer, string endPoint)
        {
            _log = _log.Combine(log) ?? (s => { });

            var buildPath = Path.Combine(BuildRoot, identifer, "Binary");
            buildPath.CreateDirectoryIfNotExis();

            var split = endPoint.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                using (_context.Repository)
                {
                    _context.GitRepo = _context.Repository.PrepareRepository(identifer);
                    BuildHost(buildPath, identifer, split[0]);

                    if (_context.SeedHostName != null) 
                        BuildSeed(buildPath, identifer, split[0], _context.SeedHostName);

                    var buildRoot = Path.Combine(BuildRoot, identifer);
                    var zip = Path.Combine(buildRoot, "binary.zip");

                    LogMessage("Create Host Zip {Id}", identifer);

                    return new BuildResult(zip, buildRoot);
                }
            }
            catch (Exception e)
            {
                LogMessage("Error: {Error}", e.Message);
                return null;
            }
        }

        private void BuildSeed(string basePath, string id, string ip, string name)
        {
            var appProject = _context.Finder.Search("Master.Seed.Node.csproj");
            if (appProject == null)
                throw new InvalidOperationException($"App Project Not Found: Master.Seed.Node.csproj");

            LogMessage("Building Seed Node {Id}", id);
            string appOutput = Path.Combine(basePath, "Seed");

            try
            {
                Directory.CreateDirectory(appOutput);

                var appResult = DotNetBuilder.BuildApplication(appProject, appOutput, s => LogMessage(s, id), _context.Configuration);
                if (!appResult)
                {
                    LogMessage("Seed Project Build Failed {Id}", id);
                    return;
                }

                var appConfig = new Configurator(appOutput);

                appConfig.SetSeed($"akka.tcp://Project-Manager@{ip}:8081");
                appConfig.SetIp(ip);
                appConfig.SetAppName(name);

                appConfig.Save();

                LogMessage("Create Zip for Seed {Id}", id);
                ZipFile.CreateFromDirectory(appOutput, Path.Combine(basePath, "Seed.zip"));

                //InstallSeed.bat
                //cd %~dp0\TestHost
                //ServiceHost.exe --Install Manual --ZipFile..\Seed.zip --AppName Master-Seed --AppType StartUp
                //pause
                //

                File.WriteAllText(Path.Combine(basePath, "InstallSeed.bat"), "cd %~dp0\\Host\n" +
                                                                             $"ServiceHost.exe --Install Manual --ZipFile ..\\Seed.zip --AppName {name} --AppType {AppType.StartUp}\n" +
                                                                             //$"del ..\\{installApp}.zip" +
                                                                             //$"del ..\\Install{installApp}.bat" +
                                                                             "pause");
            }
            finally
            {
                appOutput.DeleteDirectory(true);
            }
        }

        private void BuildHost(string basePath, string id, string ip)
        {

            var hostProject = _context.Finder.Search("ServiceHost.csproj");
            if (hostProject == null)
                throw new InvalidOperationException("Host Project Not Found");

            LogMessage("Building Host Application {Id}", id);
            string hostOutput = Path.Combine(basePath, "Host");

            Directory.CreateDirectory(hostOutput);

            var result = DotNetBuilder.BuildApplication(hostProject, hostOutput, s => LogMessage(s, id), _context.Configuration);
            if (!result)
            {
                LogMessage("Host Project Build Failed {Id}", id);
                return;
            }

            var hostConfig = new Configurator(hostOutput);
            string hostIp = ip;
            string seedIp = $"akka.tcp://Project-Manager@{ip}:8081";

            hostConfig.SetSeed(seedIp);
            hostConfig.SetIp(hostIp);
            hostConfig.SetAppName(_context.HostName);

            hostConfig.Save();

            File.WriteAllText(Path.Combine(basePath, "StartHost.bat"), "cd %~dp0\\Host\n" +
                                                                       "ServiceHost.exe\n" +
                                                                       "pause");
        }

        private sealed class RunContext
        {
            public string GitRepo { get; set; } = string.Empty;
            public string HostName { get; set; } = string.Empty;
            public string? SeedHostName { get; set; }

            public RepositoryConfiguration Configuration { get; }

            public ProjectFinder Finder => Repository.ProjectFinder;

            public ProjectManagerRepository Repository => ProjectManagerRepository.GatOrNew(Configuration);

            public RunContext(RepositoryConfiguration configuration) => Configuration = configuration;
        }
    }
}