using System;
using Akka.Routing;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Builder;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public static class ManagerBuilderExtensions
    {
        public static IConcurrentDispatcherConfugiration WithConcurentDispatcher(this ManagerBuilder builder)
        {
            var config = new ConcurrentDispatcherConfugiration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher(this ManagerBuilder builder)
        {
            var config = new ConsistentHashDispatcherConfiguration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static TConfig WithDefaultConfig<TConfig>(this IDispatcherPoolConfiguration<TConfig> config) 
            where TConfig : IDispatcherPoolConfiguration<TConfig>
        {
            return config
               .NrOfInstances(2)
               .WithResizer(new DefaultResizer(2, 10));
        }
    }
}