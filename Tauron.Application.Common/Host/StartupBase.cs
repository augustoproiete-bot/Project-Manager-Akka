using Akka.Actor;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    public abstract class StartupBase<TApplication>
    {
        protected internal abstract void ConfigureServices(ContainerBuilder builder, IConfiguration config);

        protected internal abstract void Configure(TApplication application, ActorSystem actorSystem, IContainer container);
    }
}