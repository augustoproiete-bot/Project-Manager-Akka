using Akka.Actor;
using Autofac;
using Tauron.Akka;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
using Tauron.Application.ServiceManager.ViewModels.SetupDialog;
using Tauron.Application.ServiceManager.Views;
using Tauron.Application.ServiceManager.Views.Dialogs;
using Tauron.Application.ServiceManager.Views.SetupDialogs;
using Tauron.Application.Settings;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;
using Module = Autofac.Module;

namespace Tauron.Application.ServiceManager
{
    public sealed class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CommonAppInfo>().AsSelf().SingleInstance();

            builder.RegisterType<AddSeedDialog>().As<IAddSeedUrlDialog>();
            builder.RegisterType<SelectHostAppDialog>().As<ISelectHostAppDialog>();
            builder.RegisterType<InitialDialog>().As<IInitialDialog>();

            builder.RegisterView<MainWindow, MainWindowViewModel>();
            builder.RegisterView<NodeView, NodeViewModel>();
            builder.RegisterView<SeedNodeView, SeedNodeViewModel>();
            builder.RegisterView<HostView, HostViewModel>();
            builder.RegisterView<ApplicationsView, ApplicationsViewModel>();
            builder.RegisterView<ConfigurationView, ConfigurationViewModel>();
            builder.RegisterView<SetupBuilderView, SetupBuilderViewModel>();

            builder.RegisterInstance(DialogCoordinator.Instance).As<IDialogCoordinator>();

            builder.RegisterType<AppConfiguration>().As<ISettingProviderConfiguration>();

            builder.RegisterType<DefaultActorRef<SettingsManager>>().As<IDefaultActorRef<SettingsManager>>()
               .OnActivating(i => i.Instance.Init("Settings-Manager"))
               .OnRelease(sm => sm.Actor.Tell(PoisonPill.Instance))
               .SingleInstance();
        }
    }
}