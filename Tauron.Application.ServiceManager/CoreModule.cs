using Autofac;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.ServiceManager.Views;
using Tauron.Application.Wpf;

namespace Tauron.Application.ServiceManager
{
    public sealed class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>()
               .OnActivated(a => a.Instance.Init("Main-Window"));
            builder.RegisterView<NodeView, NodeViewModel>();
        }
    }
}