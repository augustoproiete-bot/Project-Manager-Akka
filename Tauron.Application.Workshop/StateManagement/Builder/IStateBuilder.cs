using System;
using CacheManager.Core;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public interface IStateBuilder<TData>
    {
        IStateBuilder<TData> WithCache(Action<ConfigurationBuilderCachePart> cache);

        IStateBuilder<TData> WithReducer(Func<IReducer<TData>> reducer);

        IStateBuilder<TData> WithKey(string key);
    }
}