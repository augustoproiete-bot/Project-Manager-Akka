using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Tauron.Host;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public sealed class AutoViewLocation
    {
        private static readonly Dictionary<Type, Type> Views = new Dictionary<Type, Type>();

        private readonly ILifetimeScope _provider;

        public AutoViewLocation(ILifetimeScope provider) => _provider = provider;

        public static AutoViewLocation Manager => ActorApplication.Application.Continer.Resolve<AutoViewLocation>();

        public static void AddPair(Type view, Type model) 
            => Views[model] = view;

        public IView? ResolveView(object viewModel)
        {
            if (!(viewModel is IViewModel model))
                return null;

            var type = model.ModelType;
            return Views.TryGetValue(type, out var view) ? _provider.ResolveOptional(view, new TypedParameter(typeof(IViewModel<>).MakeGenericType(type), viewModel)) as IView : null;
        }
    }
}