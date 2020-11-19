using Akka.Actor;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.Model;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public sealed class ModelProperty
    {
        public Maybe<IActorRef> Model { get; }

        public UIPropertyBase Property { get; }

        public ModelProperty(Maybe<IActorRef> model, UIPropertyBase property)
        {
            Model = model;
            Property = property;
        }
    }

    [PublicAPI]
    public static class UiActorExtensions
    {
        public static ModelProperty RegisterModel<TModel>(this UiActor actor, string propertyName, string actorName)
        {
            var model = actor.LifetimeScope.Resolve<IViewModel<TModel>>();
            model.InitModel(ExposedReceiveActor.ExposedContext, May(actorName));

            return new ModelProperty(model.Actor, actor.RegisterProperty<IViewModel<TModel>>(propertyName).WithDefaultValue(model).Property.LockSet());
        }

        public static UIProperty<TData> RegisterImport<TData>(this UiActor actor, string propertyName)
            where TData : notnull
        {
            var target = actor.LifetimeScope.Resolve<TData>();
            return (UIProperty<TData>) actor.RegisterProperty<TData>(propertyName).WithDefaultValue(target).Property.LockSet();
        }
    }
}