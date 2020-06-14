using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Akka.Actor;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Boottrap
{
    public sealed class EmptyAppRoute : IAppRoute
    {
        public Task ShutdownTask { get; } = Task.CompletedTask;
        public Task WaitForStartAsync(ActorSystem actorSystem)
        {
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