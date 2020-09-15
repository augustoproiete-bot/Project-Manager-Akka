using System.Diagnostics;
using System.IO;
using System.Reflection;
using ServiceManagerIpProbe.Phases;
using Servicemnager.Networking;

namespace ServiceManagerIpProbe.Phase
{
    public sealed class SelfDestroyPhase : Phase<OperationContext>
    {
        public override void Run(OperationContext context, PhaseManager<OperationContext> manager)
        {
            context.WriteLine("Configurate Self Destroy");

            var batchCommands = string.Empty;
            var exeFileName = Assembly.GetExecutingAssembly().CodeBase?.Replace("file:///", string.Empty).Replace("/", "\\");

            batchCommands += "@ECHO OFF\n"; // Do not show any output

            batchCommands += "ping 127.0.0.1 > nul\n"; // Wait approximately 4 seconds (so that the process is already terminated)

            string[] filesToDelete = {
                                         "HostInstaller.exe.config",
                                         "HostInstaller.pdb",
                                         "SimpleTcp.dll",
                                         "Servicemnager.Networking.dll",
                                         "Servicemnager.Networking.pdb",
                                         "Interop.IWshRuntimeLibrary.dll",
                                         "Seed.zip",
                                         "InstallSeed.dat",
                                         HostConfiguration.DefaultFileName
                                     };

            batchCommands += "del /Q /F "; // Delete the executeable
            batchCommands += exeFileName + "\n";

            foreach (var file in filesToDelete)
            {
                batchCommands += "del /Q /F ";
                batchCommands += Path.GetFullPath(file) + "\n";
            }


            batchCommands += "del /Q deleteInstaller.bat"; // Delete this bat file
            //batchCommands += "pause";

            File.WriteAllText("deleteInstaller.bat", batchCommands);

            context.DestroySelf = () => Process.Start("deleteInstaller.bat");
        }
    }
}