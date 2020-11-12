using Autofac;
using Autofac.Features.ResolveAnything;
using Tauron.Akka;
using Tauron.Application;
using Tauron.Localization.Provider;

namespace Tauron
{
    public sealed class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource<AnyConcreteTypeNotAlreadyRegisteredSource>();
            builder.RegisterType<LocJsonProvider>().As<ILocStoreProducer>();

            builder.RegisterGeneric(typeof(ActorRefFactory<>)).AsSelf();
            builder.RegisterGeneric(typeof(DefaultActorRef<>)).As(typeof(IDefaultActorRef<>));
            builder.RegisterGeneric(typeof(SyncActorRef<>)).As(typeof(ISyncActorRef<>));

            builder.RegisterType<TauronEnviroment>().As<ITauronEnviroment>();
        }
    }
}