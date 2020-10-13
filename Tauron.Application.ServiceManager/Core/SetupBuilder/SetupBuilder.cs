using System;
using System.IO;
using Akka.Event;
using Tauron.Application.ServiceManager.Core.Configuration;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{

    //TODO New Setup builder
    public sealed class SetupBuilder
    {
        public static readonly string BuildRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tauron", "ServiceManager", "Build");


        //private readonly Logger _logger;
        //private readonly RunContext _context;
        //private readonly AppConfig _config;
        //private readonly EventStream? _stream;

        //private Action<string> _log;
        //private ImmutableList<string> _seeds = ImmutableList<string>.Empty;

        //private void LogMessage(string template, params object[] args)
        //    => _logger.Information(template, args);

        public SetupBuilder(string hostName, string? seedHostName, AppConfig config, Action<string> log, EventStream? stream)
        {
            //_logger = new LoggerConfiguration().WriteTo.Logger(Log.ForContext<SetupBuilder>()).WriteTo.Sink(new DelegateSink(s => _log(s))).CreateLogger();

            //_context = new RunContext(new RepositoryConfiguration(LogMessage))
            //{
            //    HostName = hostName,
            //    SeedHostName = seedHostName
            //};
            //_config = config;
            //_log = log;
            //_stream = stream;
        }

        public BuildResult? Run(Action<string> log, string identifer, string endPoint)
        {
            //_log = _log.Combine(log) ?? (s => { });

            //var buildPath = Path.Combine(BuildRoot, identifer, "Binary");
            //buildPath.CreateDirectoryIfNotExis();

            //var split = endPoint.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            //string? seedUrl = null;

            //try
            //{
            //    using (_context.Repository)
            //    {
            //        _seeds = ImmutableList<string>.Empty.AddRange(_config.SeedUrls);
            //        if (!string.IsNullOrWhiteSpace(_context.SeedHostName))
            //        {
            //            seedUrl = $"akka.tcp://Project-Manager@{split[0]}:8081";
            //            _seeds = _seeds.Add(seedUrl);
            //        }

            //        _context.Repository.PrepareRepository(identifer);
            //        BuildHost(buildPath, identifer, split[0]);

            //        if (_context.SeedHostName != null)
            //            BuildSeed(buildPath, identifer, split[0], _context.SeedHostName);

            //        var buildRoot = Path.Combine(BuildRoot, identifer);
            //        var zip = Path.Combine(buildRoot, "data.zip");

            //        LogMessage("Create Host Zip {Id}", identifer);
            //        ZipFile.CreateFromDirectory(buildPath, zip);

            //        return new BuildResult(zip, buildRoot, () => seedUrl.When(s => !string.IsNullOrWhiteSpace(s), s => _stream?.Publish(new AddSeedUrl(s!))));
            //    }
            //}
            //catch (Exception e)
            //{
            //    LogMessage("Error: {Error}", e.Message);
            //    return null;
            //}
            //finally
            //{
            //    _logger.Dispose();
            //}

            return null;
        }

        //private void BuildSeed(string basePath, string id, string ip, string name)
        //{
        //    var appProject = _context.Finder.Search("Master.Seed.Node.csproj");
        //    if (appProject == null)
        //        throw new InvalidOperationException("App Project Not Found: Master.Seed.Node.csproj");

        //    LogMessage("Building Seed Node {Id}", id);
        //    string appOutput = Path.Combine(basePath, "Seed");

        //    try
        //    {
        //        Directory.CreateDirectory(appOutput);

        //        var appResult = DotNetBuilder.BuildApplication(appProject, appOutput, s => LogMessage(s, id), _context.Configuration);
        //        if (!appResult)
        //        {
        //            LogMessage("Seed Project Build Failed {Id}", id);
        //            return;
        //        }

        //        var appConfig = new Configurator(appOutput);

        //        appConfig.SetSeed(_seeds);
        //        appConfig.SetIp(ip);
        //        appConfig.SetAppName(name);

        //        appConfig.Save();

        //        LogMessage("Create Zip for Seed {Id}", id);
        //        ZipFile.CreateFromDirectory(appOutput, Path.Combine(basePath, "Seed.zip"));

        //        //InstallSeed.bat
        //        //cd %~dp0\TestHost
        //        //ServiceHost.exe --Install Manual --ZipFile..\Seed.zip --AppName Master-Seed --AppType StartUp
        //        //pause
        //        //

        //        File.WriteAllText(Path.Combine(basePath, "InstallSeed.dat"), $"--Install Manual --ZipFile ..\\Seed.zip --AppName {name} --AppType {AppType.StartUp}");
        //    }
        //    finally
        //    {
        //        appOutput.DeleteDirectory(true);
        //    }
        //}

        //private void BuildHost(string basePath, string id, string ip)
        //{

        //    var hostProject = _context.Finder.Search("ServiceHost.csproj");
        //    if (hostProject == null)
        //        throw new InvalidOperationException("Host Project Not Found");

        //    LogMessage("Building Host Application {Id}", id);
        //    string hostOutput = Path.Combine(basePath, "Host");

        //    Directory.CreateDirectory(hostOutput);

        //    var result = DotNetBuilder.BuildApplication(hostProject, hostOutput, s => LogMessage(s, id), _context.Configuration);
        //    if (!result)
        //    {
        //        LogMessage("Host Project Build Failed {Id}", id);
        //        return;
        //    }

        //    var hostConfig = new Configurator(hostOutput);
        //    string hostIp = ip;

        //    hostConfig.SetSeed(_seeds);
        //    hostConfig.SetIp(hostIp);
        //    hostConfig.SetAppName(_context.HostName);

        //    hostConfig.Save();

        //    File.WriteAllText(Path.Combine(basePath, "StartHost.bat"), "cd %~dp0\\Host\n" +
        //                                                               "ServiceHost.exe\n" +
        //                                                               "pause");
        //}

        //private sealed class RunContext
        //{
        //    public string HostName { get; set; } = string.Empty;
        //    public string? SeedHostName { get; set; }

        //    public RepositoryConfiguration Configuration { get; }

        //    public ProjectFinder Finder => Repository.ProjectFinder;

        //    private ProjectManagerRepository? _repository;

        //    public ProjectManagerRepository Repository => _repository ??= ProjectManagerRepository.GatOrNew(Configuration);

        //    public RunContext(RepositoryConfiguration configuration) => Configuration = configuration;
        //}

        //private sealed class DelegateSink : ILogEventSink
        //{
        //    private readonly Action<string> _log;

        //    public DelegateSink(Action<string> log) => _log = log;

        //    [DebuggerHidden]
        //    public void Emit(LogEvent logEvent) => _log(logEvent.RenderMessage());
        //}
    }
}