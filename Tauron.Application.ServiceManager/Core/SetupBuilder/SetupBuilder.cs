using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
                UpdateGitRepo();    
            }
            catch (Exception e)
            {
                LogMessage("Error: {Error}", e.Message);
                return null;
            }
        }

        private IDisposable UnpackRepo()
        {
            string targetPath = Path.Combine(Env.Path, "Git\\Repo");
            targetPath.CreateDirectoryIfNotExis();
            _context.GitRepo = targetPath;

            var zip = Path.Combine(Env.Path, "Git\\Pack.tip");

            // ReSharper disable once InvertIf
            if (File.Exists(zip))
            {
                LogMessage("Upacking Git Repository");
                ZipFile.ExtractToDirectory(zip, targetPath);
            }

            return new ActionDispose(() =>
            {
                LogMessage("Packing Git Repository");

                using var clean = Process.Start(DotNetPath, $"clean {Path.Combine(targetPath, Soloution)} -c Release");
                clean?.WaitForExit();

                zip.DeleteFile();

                ZipFile.CreateFromDirectory(targetPath, zip);
                targetPath.DeleteDirectory(true);
            });
        }

        private void UpdateGitRepo()
        {
            using var repo = new GitUpdater(_context.GitRepo);

            if (repo.NeedDownload)
            {
                LogMessage("Download Project Repository");
                repo.Download();
            }
            else
            {
                repo.Update();
            }
        }

        private sealed class RunContext
        {
            public string GitRepo { get; set; } = string.Empty;
            public string HostName { get; set; } = string.Empty;
            public string? SeedHostName { get; set; }
        }
    }
}