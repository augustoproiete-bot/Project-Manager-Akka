using System;
using System.Collections.Generic;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Host;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public sealed class AutoViewLocation
    {
        private static readonly Dictionary<Type, Type> Views = new();

        private readonly ILifetimeScope _provider;

        public AutoViewLocation(ILifetimeScope provider) => _provider = provider;

        public static AutoViewLocation Manager => ActorApplication.Application.Continer.Resolve<AutoViewLocation>();

        public static void AddPair(Type view, Type model)
        {
            Views[model] = view;
        }

        public Maybe<IView> ResolveView(object viewModel)
        {
            if(viewModel is not IViewModel model)
                return Maybe<IView>.Nothing;

            var type = model.ModelType;

            return from viewType in Views.Lookup(type)
                from view in MayNotNull(_provider.ResolveOptional(viewType, new TypedParameter(typeof(IViewModel<>).MakeGenericType(type), viewModel)) as IView)
                select view;
        }
    }
}