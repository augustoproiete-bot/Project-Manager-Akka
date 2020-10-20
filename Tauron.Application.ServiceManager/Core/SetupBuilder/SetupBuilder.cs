using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Serilog;
using Serilog.Core;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.AkkNode.Services.FileTransfer.TemporarySource;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.ViewModels.ApplicationModelData;
using LogEvent = Serilog.Events.LogEvent;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{

    public sealed class SetupBuilder : IDisposable
    {
        private const string Repository = "Tauron1990/Project-Manager-Akka";
        private const string ServiceHost = "ServiceHost.csproj";
        private const string SeedNode = "Master.Seed.Node.csproj";

        private static AtomicBoolean _existRepo = new AtomicBoolean();

        public static readonly string BuildRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tauron", "ServiceManager", "Build");


        private readonly Logger _logger;
        private readonly RunContext _context;
        private readonly AppConfig _config;
        private readonly DeploymentApi _api;
        private readonly RepositoryApi _repository;
        private readonly DataTransferManager _dataTransfer;
        private readonly ActorSystem _actorSystem;

        private Action<string> _log;
        private ImmutableList<string> _seeds = ImmutableList<string>.Empty;

        private void LogMessage(string template, params object[] args)
            => _logger.Information(template, args);

        public SetupBuilder(string hostName, string? seedHostName, AppConfig config, Action<string> log, ActorSystem actorSystem, DeploymentApi api, RepositoryApi repository)
        {
            _logger = new LoggerConfiguration().WriteTo.Logger(Log.ForContext<SetupBuilder>()).WriteTo.Sink(new DelegateSink(s => _log!(s))).CreateLogger();
            _dataTransfer = DataTransferManager.New(actorSystem);

            _context = new RunContext
            {
                HostName = hostName,
                SeedHostName = seedHostName
            };
            _config = config;
            _log = log;
            _actorSystem = actorSystem;
            _api = api;
            _repository = repository;
        }

        public async Task<BuildResult?> Run(Action<string> log, string identifer, string endPoint)
        {
            _log = _log.Combine(log) ?? (s => { });

            var buildPath = Path.Combine(BuildRoot, identifer, "Binary");
            buildPath.CreateDirectoryIfNotExis();

            var split = endPoint.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            string? seedUrl = null;

            try
            {
                    _seeds = ImmutableList<string>.Empty.AddRange(_config.SeedUrls);
                    if (!string.IsNullOrWhiteSpace(_context.SeedHostName))
                    {
                        seedUrl = $"akka.tcp://Project-Manager@{split[0]}:8081";
                        _seeds = _seeds.Add(seedUrl);
                    }

                    if (await BuildHost(buildPath, identifer, split[0]))
                        return null;

                    if (_context.SeedHostName != null)
                    {
                        if(await BuildSeed(buildPath, identifer, split[0], _context.SeedHostName))
                            return null;
                    }

                    var buildRoot = Path.Combine(BuildRoot, identifer);
                    var zip = Path.Combine(buildRoot, "data.zip");

                    LogMessage("Create Host Zip {Id}", identifer);
                    ZipFile.CreateFromDirectory(buildPath, zip);

                    return new BuildResult(zip, buildRoot, () => seedUrl.When(s => !string.IsNullOrWhiteSpace(s), s => _actorSystem.EventStream?.Publish(new AddSeedUrl(s!))));
                
            }
            catch (Exception e)
            {
                LogMessage("Error: {Error}", e.Message);
                return null;
            }
            finally
            {
                _logger.Dispose();
            }
        }

        private async Task<bool> BuildSeed(string basePath, string id, string ip, string name)
        {
            await new RegisterRepository(Repository) {IgnoreDuplicate = true}
                .Send(_repository, TimeSpan.FromSeconds(20), _log);

            LogMessage("Building Seed Node {Id}", id);
            string appOutput = Path.Combine(basePath, "Seed");

            try
            {
                Directory.CreateDirectory(appOutput);

                if (await SendBuild(Repository, SeedNode, appOutput))
                {
                    _log("Seed build Failed");
                    return true;
                }

                var appConfig = new Configurator(appOutput);

                appConfig.SetSeed(_seeds);
                appConfig.SetIp(ip);
                appConfig.SetAppName(name);

                appConfig.Save();

                LogMessage("Create Zip for Seed {Id}", id);
                ZipFile.CreateFromDirectory(appOutput, Path.Combine(basePath, "Seed.zip"));


                await File.WriteAllTextAsync(Path.Combine(basePath, "InstallSeed.dat"), $"--Install Manual --ZipFile ..\\Seed.zip --AppName {name} --AppType {AppType.StartUp}");
                return false;
            }
            finally
            {
                appOutput.DeleteDirectory(true);
            }
        }

        private async Task<bool> BuildHost(string basePath, string id, string ip)
        {
            LogMessage("Building Host Application {Id}", id);
            string hostOutput = Path.Combine(basePath, "Host");

            Directory.CreateDirectory(hostOutput);

            if (await SendBuild(Repository, ServiceHost, hostOutput))
            {
                _log("Host Build Failed");
                return true;
            }

            var hostConfig = new Configurator(hostOutput);
            string hostIp = ip;

            hostConfig.SetSeed(_seeds);
            hostConfig.SetIp(hostIp);
            hostConfig.SetAppName(_context.HostName);

            hostConfig.Save();

            await File.WriteAllTextAsync(Path.Combine(basePath, "StartHost.bat"),
                "cd %~dp0\\Host\n"  +
                "ServiceHost.exe\n" +
                "pause");

            return false;
        }

        private async Task<bool> SendBuild(string repository, string projectFile, string buildPath)
        {
            

            var command = new ForceBuildCommand(repository, projectFile);

            using var tempFile = new TempFile();
            await using var zipStream = tempFile.CreateStream();
            var result = await command.Send(_api, TimeSpan.FromMinutes(5), _dataTransfer, _log, () => tempFile.CreateDate());

            if (result is TransferFailed f)
            {
                _log($"Data Transfer Failed: {f.Reason}--{f.Data}");
                return true;
            }

            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
            zip.ExtractToDirectory(buildPath, true);

            return false;
        }

        private sealed class RunContext
        {
            public string HostName { get; set; } = string.Empty;
            public string? SeedHostName { get; set; }
        }

        private sealed class DelegateSink : ILogEventSink
        {
            private readonly Action<string> _log;

            public DelegateSink(Action<string> log) => _log = log;

            [DebuggerHidden]
            public void Emit(LogEvent logEvent) => _log(logEvent.RenderMessage());
        }

        public void Dispose()
        {
            _logger.Dispose();
            _dataTransfer.Actor.Tell(PoisonPill.Instance);
        }
    }
}