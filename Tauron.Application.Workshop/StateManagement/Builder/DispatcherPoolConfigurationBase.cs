using System;
using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using Functional.Maybe;
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
        protected Maybe<Resizer> Resizer;
        protected Maybe<string> Dispatcher;
        protected Maybe<Func<Props, Props>> Custom;

        public TConfig NrOfInstances(int number)
        {
            Instances = number;
            return this.AsInstanceOf<TConfig>()!;
        }

        public TConfig WithSupervisorStrategy(SupervisorStrategy strategy)
        {
            SupervisorStrategy = strategy; 
            return this.AsInstanceOf<TConfig>()!;
        }

        public TConfig WithResizer(Resizer resizer)
        {
            Resizer = resizer.ToMaybe();
            return this.AsInstanceOf<TConfig>()!;
        }

        public TConfig WithAkkaDispatcher(string name)
        {
            Dispatcher = name.ToMaybe();
            return this.AsInstanceOf<TConfig>()!;
        }

        public TConfig WithCustomization(Func<Props, Props> custom)
        {
            Custom = Maybe.NotNull(Custom.OrElseDefault().Combine(custom));
            return this.AsInstanceOf<TConfig>()!;
        }

        public abstract IStateDispatcherConfigurator Create();
    }
}