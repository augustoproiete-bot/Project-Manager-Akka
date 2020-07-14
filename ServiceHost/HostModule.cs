using Autofac;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using Tauron.Application.AkkaNode.Boottrap;

namespace ServiceHost
{
    public sealed class HostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ManualInstallationTrigger>().As<IStartUpAction>();
            builder.RegisterType<AutoUpdater>().As<IAutoUpdater>();
            builder.RegisterType<Installer.Installer>().As<IInstaller>();
            builder.RegisterType<AppRegistry>().As<IAppRegistry>();
            base.Load(builder);
        }
    }
}