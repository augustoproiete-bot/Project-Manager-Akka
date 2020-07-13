using Autofac;
using ServiceHost.ApplicationRegistry;

namespace ServiceHost
{
    public sealed class HostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppRegistry>().As<IAppRegistry>();
            base.Load(builder);
        }
    }
}