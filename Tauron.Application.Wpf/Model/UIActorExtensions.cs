using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public static class UIActorExtensions
    {
        public static UIModel<TModel> RegisterViewModel<TModel>(this IUiActor actor, string name, Maybe<IViewModel<TModel>> mayModel = default)
            where TModel : class
        {
            var model = mayModel.OrElse(() => actor.LifetimeScope.Resolve<IViewModel<TModel>>());

            if (!model.IsInitialized)
                model.InitModel(ExposedReceiveActor.ExposedContext, May(name));

            return new UIModel<TModel>(actor.RegisterProperty<IViewModel<TModel>>(name).WithDefaultValue(model).Property);
        }

        public static FluentCollectionPropertyRegistration<TData> RegisterUiCollection<TData>(this IUiActor actor, string name)
        {
            actor.ThrowIsSeald();
            return new FluentCollectionPropertyRegistration<TData>(name, actor);
        }
    }
}