using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Serilog;
using Serilog.Parsing;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class SetupBuilder
    {
        private const string DotNetPath = @"C:\Program Files (x86)\dotnet\dotnet.exe";
        private const string Soloution = "Project-Manager-Akka.sln";

        private static readonly ILogger Logger = Log.ForContext<SetupBuilder>();
        private readonly Action<string> _log;
        private readonly RunContext _context;

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

        public SetupBuilder(string hostName, string? seedHostName, Action<string> log)
        {
            _context = new RunContext
            {
                HostName = hostName,
                SeedHostName = seedHostName
            };
            _log = log;
        }

        public string? Run(string identifer)
        {
            try
            {
                using (UnpackManager.UnpackRepo(s => LogMessage(s), _context))
                {
                    _context.GitRepo = UpdateGitRepo();
                    _context.ProjectFinder.Init(_context.GitRepo, "Project-Manager-Akka.sln", LogMessage);
                }
            }
            catch (Exception e)
            {
                LogMessage("Error: {Error}", e.Message);
                return null;
            }
        }

        private void BuildHost()
        {

        }

        private string UpdateGitRepo()
        {
            using var repo = new GitUpdater(_context.GitRepo);

            if (repo.NeedDownload)
            {
                LogMessage("Download Project Repository");
                return repo.Download();
            }

            LogMessage("Update Project Repository");
            return repo.Update();
        }

        private sealed class RunContext
        {
            public string GitRepo { get; set; } = string.Empty;
            public string HostName { get; set; } = string.Empty;
            public string? SeedHostName { get; set; }

            public ProjectFinder ProjectFinder { get; } = new ProjectFinder();
        }

        private static class UnpackManager
        {
            private static readonly object Lock = new object();
            private static int _count;

            public static IDisposable UnpackRepo(Action<string> logMessage, RunContext context)
            {
                lock (Lock)
                {
                    string targetPath = Path.Combine(Env.Path, "Git\\Repo");
                    targetPath.CreateDirectoryIfNotExis();
                    context.GitRepo = targetPath;

                    var zip = Path.Combine(Env.Path, "Git\\Pack.tip");


                    if (_count == 0)
                    {
                        // ReSharper disable once InvertIf
                        if (File.Exists(zip))
                        {
                            logMessage("Upacking Git Repository");
                            ZipFile.ExtractToDirectory(zip, targetPath);
                        }
                    }
                    else
                        logMessage("Repository is Upacked");

                    _count++;

                    return new ActionDispose(() =>
                    {
                        lock (Lock)
                        {
                            _count--;

                            if (_count != 0) return;
                            
                            logMessage("Packing Git Repository");

                            using var clean = Process.Start(DotNetPath, $"clean {Path.Combine(targetPath, Soloution)} -c Release");
                            clean?.WaitForExit();

                            zip.DeleteFile();

                            ZipFile.CreateFromDirectory(targetPath, zip);
                            targetPath.DeleteDirectory(true);
                        }
                    });
                }
            }
        }
    }
}