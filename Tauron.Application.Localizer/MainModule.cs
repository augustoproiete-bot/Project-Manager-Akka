using Autofac;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.Core.UI;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Localizer.Views;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<CenterView, CenterViewModel>();
            builder.RegisterView<ProjectView, ProjectViewModel>().InstancePerDependency();
            builder.RegisterView<MainWindow, MainWindowViewModel>().OnActivating(i => i.Instance.Init("Main-Window"));

            builder.RegisterType<OpenFileDialogView>().As<IOpenFileDialog>();
            builder.RegisterType<NewProjectDialogView>().As<IProjectNameDialog>();
            builder.RegisterType<MainWindowCoordinator>().As<IMainWindowCoordinator>().SingleInstance();

            builder.RegisterType<LocLocalizer>().AsSelf();

            builder.RegisterInstance(DialogCoordinator.Instance).As<IDialogCoordinator>();
        }
    }
}