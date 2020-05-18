using System;
using Akka.Actor;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Model;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class AutofacExtensions
    {
        public static IRegistrationBuilder<ViewModelActorRef<TModel>, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterView<TView, TModel>(this  ContainerBuilder builder)
            where TView : IView where TModel : ActorBase
        {
            AutoViewLocation.AddPair(typeof(TView), typeof(TModel));

            builder.RegisterType<TView>().As<TView>();
            return builder.RegisterType<ViewModelActorRef<TModel>>().As<IViewModel<TModel>>().Keyed<IViewModel>(typeof(TModel)).InstancePerLifetimeScope()
                .OnRelease(vm =>
                {
                    if(vm.IsInitialized)
                        vm.Actor.Tell(PoisonPill.Instance);
                });
        }

        public static IRegistrationBuilder<DefaultActorRef<TActor>, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterDefaultActor<TActor>(this ContainerBuilder builder) 
            where TActor : ActorBase =>
            builder.RegisterType<DefaultActorRef<TActor>>().As<IDefaultActorRef<TActor>>();

        public static IRegistrationBuilder<SyncActorRef<TActor>, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSyncActor<TActor>(this ContainerBuilder builder)
            where TActor : ActorBase =>
            builder.RegisterType<SyncActorRef<TActor>>().As<ISyncActorRef<TActor>>();

        public static IRegistrationBuilder<DefaultActorRef<TActor>, SimpleActivatorData, SingleRegistrationStyle> RegisterDefaultActor<TActor>(this ContainerBuilder builder, 
            Func<IComponentContext, DefaultActorRef<TActor>> fac) where TActor : ActorBase =>
            builder.Register(fac).As<IDefaultActorRef<TActor>>();

        public static IRegistrationBuilder<SyncActorRef<TActor>, SimpleActivatorData, SingleRegistrationStyle> RegisterSyncActor<TActor>(this ContainerBuilder builder, 
            Func<IComponentContext, SyncActorRef<TActor>> fac) where TActor : ActorBase =>
            builder.Register(fac).As<ISyncActorRef<TActor>>();
    }
}