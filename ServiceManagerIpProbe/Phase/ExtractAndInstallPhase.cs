using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using ServiceManagerIpProbe.Phases;
using File = System.IO.File;

namespace ServiceManagerIpProbe.Phase
{
    public class ExtractAndInstallPhase : Phase<OperationContext>
    {
        public override void Run(OperationContext context, PhaseManager<OperationContext> manager)
        {
            Console.WriteLine("Extract Application Data");
            var appDic = Path.GetDirectoryName(Application.ExecutablePath);

            if(string.IsNullOrWhiteSpace(appDic))
                throw new InvalidOperationException("Executable Path not found");

            using (var zip = ZipFile.OpenRead(context.TargetFile)) 
                zip.ExtractToDirectory(appDic);

            var seedBat = Path.Combine(appDic, "InstallSeed.bat");

            if (File.Exists(seedBat))
            {
                Console.WriteLine("Installing Seed");

                using (var process = Process.Start(seedBat))
                {
                    if(process == null)
                        Console.WriteLine("Seed Install Failed");
                    else
                    {
                        while (process.WaitForExit(5000))
                        {
                            if(context.GlobalTimeout.IsCancellationRequested)
                                return;
                        }

                        Console.WriteLine("Seed Installation Ok");
                    }
                }
            }

            Console.WriteLine("Starting Host");
            var startBat = Path.Combine(appDic, "StartHost.bat");
            if (File.Exists(startBat)) 
                Process.Start(startBat)?.Dispose();

            Console.WriteLine("Try Creating Start Shortcut");

            try
            {
                var startFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                IWshShell shellClass = new WshShellClass();

                //Create Desktop Shortcut for Application Settings
                var startLink = Path.Combine(startFolder, "AppHost.lnk");

                if(File.Exists(startLink))
                    File.Delete(startLink);

                var shortcut = (IWshShortcut)shellClass.CreateShortcut(startLink);
                shortcut.TargetPath = Path.Combine(appDic, "Host", "ServiceHost.exe");
                //shortcut.IconLocation = @"C:\Program FilesMorganTechSpacesettings.ico";
                //shortcut.Arguments = "arg1 arg2";
                shortcut.Description = "Click to edit MorganApp settings";
                shortcut.Save();
            }
            catch (Exception e)
            {
                Console.WriteLine("Creation Failed");
                Console.WriteLine(e.Message);
            }

            manager.RunNext(context);
        }
    }
}