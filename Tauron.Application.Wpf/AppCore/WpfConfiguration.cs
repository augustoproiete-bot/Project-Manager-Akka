using System;
using Autofac;
using JetBrains.Annotations;
using Tauron.Host;

namespace Tauron.Application.Wpf.AppCore
{
    [PublicAPI]
    public sealed class WpfConfiguration
    {
        internal readonly ContainerBuilder ServiceCollection;

        public WpfConfiguration(ContainerBuilder serviceCollection) => ServiceCollection = serviceCollection;

        public WpfConfiguration WithAppFactory(Func<System.Windows.Application> factory)
        {
            ServiceCollection.Register(context => new DelegateAppFactory(factory)).As<IAppFactory>().IfNotRegistered(typeof(IAppFactory));
            return this;
        }

        public WpfConfiguration WithRoute<TRoute>(string name)
            where TRoute : class, IAppRoute
        {
            ServiceCollection.RegisterType<TRoute>().Named<IAppRoute>(name);
            return this;
        }
    }
}