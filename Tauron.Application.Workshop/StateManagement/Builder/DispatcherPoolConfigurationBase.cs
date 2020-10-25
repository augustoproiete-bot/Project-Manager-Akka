using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    [PublicAPI]
    public abstract class DispatcherPoolConfigurationBase<TConfig> : IDispatcherPoolConfiguration<TConfig> 
        where TConfig : class, IDispatcherPoolConfiguration<TConfig>
    {
        protected int Instances = 2;
        protected SupervisorStrategy SupervisorStrategy = Pool.DefaultSupervisorStrategy;
        protected Resizer? Resizer;
        protected string? Dispatcher;

        public TConfig NrOfInstances(int number)
        {
            Instances = number;
            return this.As<TConfig>()!;
        }

        public TConfig WithSupervisorStrategy(SupervisorStrategy strategy)
        {
            SupervisorStrategy = strategy; 
            return this.As<TConfig>()!;
        }

        public TConfig WithResizer(Resizer resizer)
        {
            Resizer = resizer;
            return this.As<TConfig>()!;
        }

        public TConfig WithAkkaDispatcher(string name)
        {
            Dispatcher = name;
            return this.As<TConfig>()!;
        }

        public abstract IStateDispatcherConfigurator Create();
    }
}