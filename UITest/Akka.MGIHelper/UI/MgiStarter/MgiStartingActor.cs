using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using Akka.MGIHelper.Core.Configuration;
using IniParser;
using Tauron.Application.Wpf;
using Tauron.Localization;

namespace Akka.MGIHelper.UI.MgiStarter
{
    public sealed class MgiStartingActor : ReceiveActor
    {
        private readonly IDialogFactory _dialogFactory;

        private readonly ILoggingAdapter _log = Context.GetLogger();

        public MgiStartingActor(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
            Receive<TryStart>(TryStartHandler);
        }

        private void TryStartHandler(TryStart obj)
        {
            try
            {
                _log.Info("Start Mgi Process");

                var config = obj.Config;
                Sender.Tell(new StartStatusUpdate(Context.Loc().RequestString("kernelstartstartlabel")));
                obj.Kill();

                Thread.Sleep(500);
                if (!CheckKernelRunning(config, out var target))
                {
                    _dialogFactory.ShowMessageBox(null, Context.Loc().RequestString("kernelstarterror"), "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
                    try
                    {
                        target.Kill(true);
                        target.Dispose();
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, "Error on Kill Kernel");
                    }

                    return;
                }

                Thread.Sleep(500);
                Directory.SetCurrentDirectory(Path.GetDirectoryName(config.Client.Trim()));
                Process.Start(config.Client);

                Sender.Tell(new StartStatusUpdate(Context.Loc().RequestString("kernelstartcompledlabel")));
            }
            catch (Exception e)
            {
                _log.Warning(e, "Error on Start Mgi process");
                _dialogFactory.FormatException(null, e);
            }
            finally
            {
                Sender.Tell(new TryStartResponse());
            }
        }

        private bool CheckKernelRunning(ProcessConfig config, out Process kernel)
        {
            var kernelPath = config.Kernel.Trim();
            var statusPath = Path.Combine(Path.GetDirectoryName(kernelPath) ?? string.Empty, "Status.ini");
            const int iterationCount = 60;
            var parser = new FileIniDataParser();

            if (File.Exists(statusPath))
                File.Delete(statusPath);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(kernelPath));
            kernel = Process.Start(kernelPath);

            Thread.Sleep(5000);

            for (var i = 0; i < iterationCount; i++)
                try
                {
                    Thread.Sleep(1100);

                    var data = parser.ReadFile(statusPath);
                    var status = data.Sections["Status"].GetKeyData("Global").Value;
                    switch (status)
                    {
                        case "Ready":
                            return true;
                        case "Fail":
                        case "Close":
                            return false;
                    }
                }
                catch
                {
                    Thread.Sleep(500);
                }

            return false;
        }

        public sealed class TryStart
        {
            public TryStart(ProcessConfig config, Action kill)
            {
                Config = config;
                Kill = kill;
            }

            public ProcessConfig Config { get; }

            public Action Kill { get; }
        }

        public sealed class TryStartResponse
        {
        }

        public sealed class StartStatusUpdate
        {
            public StartStatusUpdate(string status)
            {
                Status = status;
            }

            public string Status { get; }
        }
    }
}