using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Boottrap
{
    public sealed class EmptyAppRoute : IAppRoute
    {
        public Task ShutdownTask { get; private set; } = Task.CompletedTask;
        public Task WaitForStartAsync(ActorSystem actorSystem)
        {
            var source = new TaskCompletionSource<object>();
            ShutdownTask = source.Task;

            actorSystem.RegisterOnTermination(() => source.SetResult(new object()));

            Maximize();
            return Task.CompletedTask;
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }
    }
}