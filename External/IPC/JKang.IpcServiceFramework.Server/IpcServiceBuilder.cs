using System;
using Autofac;

namespace JKang.IpcServiceFramework
{
    internal class IpcServiceBuilder : IIpcServiceBuilder
    {
        public IpcServiceBuilder(ContainerBuilder services) => Services = services;

        public ContainerBuilder Services { get; }

        public IIpcServiceBuilder AddService<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Services.RegisterType<TImplementation>().As<TInterface>().InstancePerLifetimeScope();
            return this;
        }

        public IIpcServiceBuilder AddService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Services.RegisterType<TImplementation>().As<TInterface>().InstancePerLifetimeScope();
            return this;
        }

        public IIpcServiceBuilder AddService<TInterface>(Func<IServiceProvider, TInterface> implementationFactory)
            where TInterface : class
        {
            Services.AddScoped(implementationFactory);
            return this;
        }
    }
}