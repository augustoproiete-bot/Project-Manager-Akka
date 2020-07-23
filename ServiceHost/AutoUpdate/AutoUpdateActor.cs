using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Tauron;
using Tauron.Akka;

namespace ServiceHost.AutoUpdate
{
    [UsedImplicitly]
    public sealed class AutoUpdateActor : ExposedReceiveActor
    {
        private const string UpdaterExe = "AutoUpdateRunner.exe";
        private static readonly string UpdatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron", "PMHost");
        private static readonly string UpdateZip = Path.Combine(UpdatePath, "update.zip");

        public AutoUpdateActor()
        {
            Receive<StartCleanUp>(CleanUp);
            Receive<StartAutoUpdate>(AutoUpdate);
        }

        private void AutoUpdate(StartAutoUpdate obj)
        {
            try
            {
                Log.Info("Try Start Auto Update");

                UpdatePath.CreateDirectoryIfNotExis();
                File.Move(obj.OriginalZip, UpdateZip, true);
                string hostPath = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.CodeBase) ?? string.Empty).LocalPath;
                string autoUpdateExe = Path.Combine(UpdatePath, UpdaterExe);
                if (!hostPath.ExisDirectory())
                {
                    Log.Warning("host Path Location not found");
                    return;
                }

                var info = new SetupInfo(UpdateZip, 
                    "ServiceHost.exe", 
                    hostPath, 
                    Process.GetCurrentProcess().Id);

                File.Copy(Path.Combine(hostPath, UpdaterExe), autoUpdateExe, true);

                Process.Start(new ProcessStartInfo(autoUpdateExe, info.ToCommandLine()) { WorkingDirectory = UpdatePath });
                Context.System.Terminate();
            }
            catch (Exception e)
            {
                Log.Info(e, "Error on Start Auto Update");
            }
        }

        private void CleanUp(StartCleanUp obj)
        {
            try
            {
                Log.Info("Cleanup after Auto Update");
                KillProcess(obj.Id);

                UpdateZip.DeleteFile();
                UpdatePath.DeleteDirectory(true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Clean up Auto update file");
            }
        }

        private void KillProcess(int id)
        {
            Log.Info("Killing Update process");
            try
            {
                using var process = Process.GetProcessById(id);
                var time = 60000;

                while (!process.HasExited)
                {
                    Thread.Sleep(1000);
                    time -= 1000;

                    if (time >= 0) continue;

                    process.Kill(true);
                    break;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Getting Update Process");
            }
        }
    }
}