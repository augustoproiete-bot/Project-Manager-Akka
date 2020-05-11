using Autofac;
using Tauron.Application.Localizer.ViewModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>();
        }
    }
}