using System;
using System.IO;
using System.Linq;
using CacheManager.Core;

namespace AkkaTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using var cache = CacheFactory.Build<string>(part =>
            {
                part.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(30));
                part.WithSystemRuntimeCacheHandle(true).EnableStatistics().WithExpiration(ExpirationMode.Sliding, TimeSpan.FromMinutes(10));
            });

            cache.Add(new CacheItem<string>("Test", "Test").WithNoExpiration());
        }
    }
}