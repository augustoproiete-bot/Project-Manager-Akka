using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Helper;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public static class UIActorExtensions
    {
        public static UIModel<TModel> RegisterViewModel<TModel>(this UiActor actor, string name, IViewModel<TModel>? model = null)
            where TModel : class
        {
            model ??= actor.LifetimeScope.Resolve<IViewModel<TModel>>();

            if (!model.IsInitialized)
                model.InitModel(actor.ExposedContext, name);

            return new UIModel<TModel>(actor.RegisterProperty<IViewModel<TModel>>(name).WithDefaultValue(model).Property);
        }

        public static FluentCollectionPropertyRegistration<TData> RegisterUiCollection<TData>(this UiActor actor, string name)
        {
            actor.ThrowIsSeald();
            return new FluentCollectionPropertyRegistration<TData>(name, actor);
        }
    }
}