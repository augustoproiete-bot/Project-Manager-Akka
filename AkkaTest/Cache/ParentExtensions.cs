using CacheManager.Core;

namespace AkkaTest.Cache
{
    public static class ParentExtensions
    {
        public static ConfigurationBuilderCachePart AddParentCache<TCacheValue, TParentValue>(this ConfigurationBuilderCachePart config, ICache<TParentValue> cache);
    }
}