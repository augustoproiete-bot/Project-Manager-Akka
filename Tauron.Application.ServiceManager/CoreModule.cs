using Autofac;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.ServiceManager
{
    public sealed class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>();
        }
    }
}