using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using Tauron.Host;

namespace Tauron.Application.Wpf.AppCore
{
    public sealed class AppLifetime : IAppRoute
    {
        private readonly ILifetimeScope _factory;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private System.Windows.Application? _internalApplication;
        private readonly TaskCompletionSource<int> _shutdownWaiter = new TaskCompletionSource<int>();

        public AppLifetime(ILifetimeScope factory, IHostEnvironment hostEnvironment, IHostApplicationLifetime applicationLifetime)
        {
            _factory = factory;
            _hostEnvironment = hostEnvironment;
            _applicationLifetime = applicationLifetime;
        }

        public Task WaitForStartAsync(ActorSystem system)
        {
            void ShutdownApp()
            {
                _internalApplication?.Dispatcher.Invoke(_internalApplication.Shutdown);
                _applicationLifetime.StopApplication();
            }

            void Runner()
            {
                using var scope = _factory.BeginLifetimeScope();

                _internalApplication = scope.Resolve<IAppFactory>()?.Create() ?? new System.Windows.Application();

                _internalApplication.Startup += (sender, args) =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    var splash = scope.ResolveOptional<ISplashScreen>()?.Window;
                    splash?.Show();

                    var mainWindow = scope.Resolve<IMainWindow>();
                    mainWindow.Window.Show();
                    mainWindow.Shutdown += (o, eventArgs) 
                        => ShutdownApp();

                    splash?.Hide();
                    // ReSharper restore AccessToDisposedClosure
                };

                system.RegisterOnTermination(() => _internalApplication.Shutdown(0));

                _shutdownWaiter.SetResult(_internalApplication.Run());
            }

            Thread uiThread = new Thread(Runner)
                              {
                                  Name = "UI Thread",
                                  IsBackground = true
                              };
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            return Task.CompletedTask;
        }

        public Task ShutdownTask => _shutdownWaiter.Task;
    }
}