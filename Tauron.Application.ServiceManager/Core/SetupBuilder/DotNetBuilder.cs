using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ServiceManager.ProjectRepository;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public static class DotNetBuilder 
    {
        public static bool BuildApplication(FileInfo project, string output, Action<string> log, RepositoryConfiguration configuration)
        {
            var arguments = new StringBuilder()
               .Append(" publish ")
               .Append($"\"{project.FullName}\"")
               .Append($" -o \"{output}\"")
               .Append(" -c Release")
               .Append(" -v m")
               .Append(" --no-self-contained");

            using var process = new Process {StartInfo = new ProcessStartInfo(configuration.DotNetPath, arguments.ToString())
                                                         {
                                                             UseShellExecute = false
                                                         }};



            log("Start Project Build  Process {Id}");
            process.Start();

            Thread.Sleep(1000);

            //_log.Information("Wait For Exit");
            if (!process.WaitForExit(30000))
            {
                log("Killing Process {Id}");
                process.Kill(true);
            }

            log("Build Compled {id}");
            return process.ExitCode == 0;
        }
    }
}