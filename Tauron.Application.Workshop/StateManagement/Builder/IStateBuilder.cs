using System;
using CacheManager.Core;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    [PublicAPI]
    public interface IStateBuilder<TData>
        where TData : class
    {
        IStateBuilder<TData> WithStateType<TState>()
            where TState : IState<TData>;
        
        IStateBuilder<TData> WithNoCache();

        IStateBuilder<TData> WithParentCache();

        IStateBuilder<TData> WithCache(Action<ConfigurationBuilderCachePart> cache);

        IStateBuilder<TData> WithReducer(Func<IReducer<TData>> reducer);

        IStateBuilder<TData> WithKey(string key);
    }
}