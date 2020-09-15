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
            context.WriteLine("Extract Application Data");
            var appDic = Path.GetDirectoryName(Application.ExecutablePath);

            if(string.IsNullOrWhiteSpace(appDic))
                throw new InvalidOperationException("Executable Path not found");

            using (var zip = ZipFile.OpenRead(context.TargetFile)) 
                zip.ExtractToDirectory(appDic);

            var seedBat = Path.Combine(appDic, "InstallSeed.dat");
            var seedZip = Path.Combine(appDic, "Seed.zip");

            if (File.Exists(seedBat))
            {
                context.WriteLine("Installing Seed");

                using (var process = Process.Start(new ProcessStartInfo(Path.Combine(appDic, "Host", "ServiceHost.exe"))
                {
                    Arguments = File.ReadAllText(seedBat),
                    WorkingDirectory = Path.Combine(appDic, "Host")
                }))
                {
                    if (process == null)
                        context.WriteLine("Seed Install Failed");
                    else
                    {
                        while (!process.WaitForExit(5000))
                        {
                            if (context.GlobalTimeout.IsCancellationRequested)
                                return;
                        }

                        context.WriteLine("Seed Installation Ok");
                    }
                }
            }

            context.WriteLine("Starting Host");
            var hostApp = Path.Combine(appDic, "Host", "ServiceHost.exe");
            if (File.Exists(hostApp)) 
                Process.Start(new ProcessStartInfo(hostApp)
                {
                    WorkingDirectory = Path.Combine(appDic, "Host")
                })?.Dispose();

            if (context.Configuration.CreateShortcut)
            {
                context.WriteLine("Try Creating Start Shortcut");

                try
                {
                    var startFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    IWshShell shellClass = new WshShellClass();

                    //Create Desktop Shortcut for Application Settings
                    var startLink = Path.Combine(startFolder, "AppHost.lnk");

                    if (File.Exists(startLink))
                        File.Delete(startLink);

                    var shortcut = (IWshShortcut) shellClass.CreateShortcut(startLink);
                    shortcut.TargetPath = Path.Combine(appDic, "Host", "ServiceHost.exe");
                    //shortcut.IconLocation = @"C:\Program FilesMorganTechSpacesettings.ico";
                    //shortcut.Arguments = "arg1 arg2";
                    shortcut.Description = "Click to edit MorganApp settings";
                    shortcut.Save();
                }
                catch (Exception e)
                {
                    context.WriteLine("Creation Failed");
                    context.WriteLine(e.Message);
                }
            }

            manager.RunNext(context);
        }
    }
}