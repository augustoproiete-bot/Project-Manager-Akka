using Akka.Actor;
using Autofac;
using Tauron.Akka;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.ServiceManager.Views;
using Tauron.Application.Settings;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.ServiceManager
{
    public sealed class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>()
               .OnActivated(a => a.Instance.Init("Main-Window"));
            builder.RegisterView<NodeView, NodeViewModel>();
            builder.RegisterView<SeedNodeView, SeedNodeViewModel>();

            builder.RegisterInstance(DialogCoordinator.Instance).As<IDialogCoordinator>();

            builder.RegisterType<AppConfiguration>().As<ISettingProviderConfiguration>();

            builder.RegisterType<DefaultActorRef<SettingsManager>>().As<IDefaultActorRef<SettingsManager>>()
               .OnActivating(i => i.Instance.Init("Settings-Manager"))
               .OnRelease(sm => sm.Actor.Tell(PoisonPill.Instance))
               .SingleInstance();
        }
    }
}