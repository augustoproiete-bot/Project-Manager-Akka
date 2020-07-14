using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Akka.Actor;
using Serilog;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Boottrap
{
    public sealed class ConsoleAppRoute : IAppRoute
    {
        private IStartUpAction[] _actions;

        public ConsoleAppRoute(IEnumerable<IStartUpAction> startUpActions)
        {
            _actions = startUpActions.ToArray();
        }

        public Task ShutdownTask { get; private set; } = Task.CompletedTask;
        public Task WaitForStartAsync(ActorSystem actorSystem)
        {
            var source = new TaskCompletionSource<object>();
            ShutdownTask = source.Task;

            actorSystem.RegisterOnTermination(() => source.SetResult(new object()));

            Maximize();
            Task.Run(() =>
            {
                foreach (var startUpAction in _actions)
                {
                    try
                    {
                        startUpAction.Run();
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Error(e, "Error on Startup Action");
                    }
                }

                _actions = null;
            });
            return Task.CompletedTask;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }
    }
}