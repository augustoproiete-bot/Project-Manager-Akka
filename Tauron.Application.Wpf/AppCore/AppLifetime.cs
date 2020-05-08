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

                LogManager.AddListener(new CatelListner(scope.ServiceProvider.GetRequiredService<ILogger<CatelListner>>()));
                IocReplacer.SetServiceProvider(scope.ServiceProvider);

                _internalApplication = scope.ServiceProvider.GetService<IAppFactory>()?.Create() ?? new System.Windows.Application();

                _internalApplication.Startup += (sender, args) =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    var splash = scope.ServiceProvider.GetService<ISplashScreen>()?.Window;
                    splash?.Show();

                    scope.ServiceProvider.GetService<WpfStartup>()?.Configure(_internalApplication, _hostEnvironment);

                    var mainWindow = scope.ServiceProvider.GetRequiredService<IMainWindow>();
                    mainWindow.Window.Show();
                    mainWindow.Shutdown += (o, eventArgs) 
                        => ShutdownApp();

                    scope.ServiceProvider.GetRequiredService<IWpfLifetime>().ShutdownEvent += (o, eventArgs) 
                        => ShutdownApp();

                    splash?.Hide();
                    // ReSharper restore AccessToDisposedClosure
                };

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