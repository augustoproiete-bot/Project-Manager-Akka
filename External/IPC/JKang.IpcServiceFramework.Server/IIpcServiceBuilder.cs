using System;
using Autofac;
using JetBrains.Annotations;

namespace JKang.IpcServiceFramework
{
    [PublicAPI]
    public interface IIpcServiceBuilder
    {
        ContainerBuilder Services { get; }

        IIpcServiceBuilder AddService<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        IIpcServiceBuilder AddService<TInterface, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory)
            where TInterface : class
            where TImplementation : class, TInterface;

        IIpcServiceBuilder AddService<TInterface>(Func<IServiceProvider, TInterface> implementationFactory)
            where TInterface : class;
    }
}