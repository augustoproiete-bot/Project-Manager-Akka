using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tauron.Application.Master.Commands.Deployment.Build;

namespace ServiceManager.ProjectDeployment.Build
{
    public static class DotNetBuilder 
    {
        public static Task<string?> BuildApplication(FileInfo project, string output, Action<string> log) 
            => Task.Run(async () => await BuildApplicationAsync(project, output, log));

        private static async Task<string?> BuildApplicationAsync(FileInfo project, string output, Action<string> log)
        {
            var arguments = new StringBuilder()
               .Append(" publish ")
               .Append($"\"{project.FullName}\"")
               .Append($" -o \"{output}\"")
               .Append(" -c Release")
               .Append(" -v m")
               .Append(" --no-self-contained");

            var path = DedectDotNet(project);
            if (string.IsNullOrWhiteSpace(path))
                return BuildErrorCodes.BuildDotnetNotFound;

            using var process = new Process {StartInfo = new ProcessStartInfo(path, arguments.ToString())
                                                         {
                                                             UseShellExecute = false,
                                                             CreateNoWindow = true
                                                         }};



            log(DeploymentMessages.BuildStart);
            process.Start();

            await Task.Delay(5000);

            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(2000);
                if(process.HasExited) break;
            }

            if (!process.HasExited)
            {
                log(DeploymentMessages.BuildKilling);
                process.Kill();
                return BuildErrorCodes.BuildDotNetFailed;
            }

            log(DeploymentMessages.BuildCompled);
            return process.ExitCode == 0 ? null : BuildErrorCodes.BuildDotNetFailed;
        }

        private static string? DedectDotNet(FileInfo project)
        {
            const string netcoreapp = "netcoreapp";
            const string netstandard = "netstandard";
            const string net = "net";

            var data = XElement.Load(project.FullName);
            var frameWork = data
               .Elements("PropertyGroup")
               .SelectMany(e => e.Elements())
               .FirstOrDefault(e => e.Name == "TargetFramework")
              ?.Value;

            if (frameWork == null) return null;

            string searchTerm;

            if (frameWork.StartsWith(netcoreapp)) searchTerm = frameWork.Replace(netcoreapp, string.Empty);
            else if (frameWork.StartsWith(netstandard)) searchTerm = frameWork.Replace(netstandard, string.Empty);
            else searchTerm = frameWork.Replace(net, string.Empty);

            searchTerm = searchTerm.Substring(0, 3);

            var enviromentPaths = Environment.GetEnvironmentVariable("Path")?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var dotnetPaths = enviromentPaths?.Where(Directory.Exists).SelectMany(Directory.EnumerateFiles).Where(f => f.EndsWith("dotnet.exe")).ToArray();

            return dotnetPaths?.FirstOrDefault(p =>
            {
                var sdkPath = Path.Combine(Path.GetDirectoryName(p) ?? string.Empty, "sdk");
                return Directory.Exists(sdkPath) && Directory.EnumerateDirectories(sdkPath).Any(e => new DirectoryInfo(e).Name.StartsWith(searchTerm));
            });
        }
    }
}