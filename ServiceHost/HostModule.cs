using Autofac;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using ServiceHost.Services;
using Tauron.Application.AkkaNode.Boottrap;

namespace ServiceHost
{
    public sealed class HostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServiceManager>().As<IServiceManager>();
            builder.RegisterType<AutoUpdater>().As<IAutoUpdater>();
            builder.RegisterType<Installer.Installer>().As<IInstaller>();
            builder.RegisterType<AppRegistry>().As<IAppRegistry>();

            builder.RegisterType<ManualInstallationTrigger>().As<IStartUpAction>();

            base.Load(builder);
        }
    }
}