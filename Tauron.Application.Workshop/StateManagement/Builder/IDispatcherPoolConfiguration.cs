using System;
using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    [PublicAPI]
    public interface IDispatcherPoolConfiguration<out TConfig>
        where TConfig : IDispatcherPoolConfiguration<TConfig>
    {
        TConfig NrOfInstances(int number);

        TConfig WithSupervisorStrategy(SupervisorStrategy strategy);

        TConfig WithResizer(Resizer resizer);

        TConfig WithAkkaDispatcher(string name);

        TConfig WithCustomization(Func<Props, Props> custom);
    }
}