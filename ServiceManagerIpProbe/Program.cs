using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using SimpleTcp;

namespace ServiceManagerIpProbe
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Ip Probe";

            try
            {
                var config = JsonConvert.DeserializeObject<IpProbeConfiguration>(File.ReadAllText(IpProbeConfiguration.DefaultFileName));

                var data = config.TargetAdress.Split(':');

                var port = int.Parse(data[1]);

                using (var client = new SimpleTcpClient(data[0], port, false, null, null))
                {
                    client.Connect();
                    client.Send(config.Identifer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(e);
            }
            
            Thread.Sleep(4000);

            var batchCommands = string.Empty;
            var exeFileName = Assembly.GetExecutingAssembly().CodeBase?.Replace("file:///", string.Empty).Replace("/", "\\");

            batchCommands += "@ECHO OFF\n";                         // Do not show any output

            batchCommands += "ping 127.0.0.1 > nul\n";              // Wait approximately 4 seconds (so that the process is already terminated)

            string[] filesToDelete = new[]
            {
                exeFileName,
                IpProbeConfiguration.DefaultFileName,
                "Newtonsoft.Json.dll",
                "ServiceManagerIpProbe.exe.config",
                "ServiceManagerIpProbe.pdb",
                "SimpleTcp.dll"
            };

            batchCommands += "del /Q /F ";                    // Delete the executeable
            batchCommands += exeFileName + "\n";

            foreach (var file in filesToDelete)
            {
                batchCommands += "del /Q /F ";
                batchCommands += Path.GetFullPath(file) + "\n";
            }


            batchCommands += "del /Q deleteProbe.bat";    // Delete this bat file

            File.WriteAllText("deleteProbe.bat", batchCommands);

            Process.Start("deleteProbe.bat");
        }
    }
}
