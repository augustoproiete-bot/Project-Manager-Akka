using Autofac;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using ServiceHost.Services;
using ServiceHost.SharedApi;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceHost
{
    public sealed class HostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppManager>().As<IAppManager>().SingleInstance();
            builder.RegisterType<AutoUpdater>().As<IAutoUpdater>().SingleInstance();
            builder.RegisterType<Installer.Installer>().As<IInstaller>().SingleInstance();
            builder.RegisterType<AppRegistry>().As<IAppRegistry>().SingleInstance();

            builder.RegisterType<ManualInstallationTrigger>().As<IStartUpAction>();
            builder.RegisterType<ServiceStartupTrigger>().As<IStartUpAction>();
            builder.RegisterType<CleanUpDedector>().As<IStartUpAction>();
            builder.RegisterType<ApiDispatcherStartup>().As<IStartUpAction>();

            builder.RegisterType<InstallChecker>().AsSelf();

            base.Load(builder);
        }
    }
}