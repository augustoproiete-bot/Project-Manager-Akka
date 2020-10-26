using CacheManager.Core;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Cache
{
    [PublicAPI]
    public static class ParentExtensions
    {
        public static ConfigurationBuilderCacheHandlePart WithParentCache<TCacheValue>(this ConfigurationBuilderCachePart config, ParentCache<TCacheValue> parent) 
            => config.WithHandle(typeof(ParentCacheHandle<>), "PraentCache", false, parent);
    }
}