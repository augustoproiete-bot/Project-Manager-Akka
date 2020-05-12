using Autofac;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Localizer.Views;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>().OnActivating(i => i.Instance.Init("Main-Window"));
            builder.RegisterType<OpenFileDialogView>().As<IOpenFileView>();

            builder.RegisterType<LocLocalizer>().AsSelf();

            builder.RegisterInstance(DialogCoordinator.Instance).As<IDialogCoordinator>();
        }
    }
}