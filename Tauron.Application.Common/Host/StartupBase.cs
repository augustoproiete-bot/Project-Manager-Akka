using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Tauron.Host
{
    [PublicAPI]
    public abstract class StartupBase<TApplication>
    {
        protected internal abstract void ConfigureServices(ContainerBuilder builder, IConfiguration config);

        protected internal abstract void Configure(TApplication application, ActorSystem actorSystem, IContainer container);
    }
}