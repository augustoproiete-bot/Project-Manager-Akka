using System.Collections.Generic;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class CollectionExtensions
    {
        public static Maybe<TType> TryRemove<TKey, TType>(this IDictionary<TKey, TType> dictionary, TKey key)
            where TKey : notnull
            => dictionary.Remove(key, out var value) ? value.ToMaybe() : Maybe<TType>.Nothing;

        public static Maybe<TType> TryGetValue<TKey, TType>(this IDictionary<TKey, TType> dictionary, TKey key)
            where TKey : notnull
            => dictionary.TryGetValue(key, out var value) ? value.ToMaybe() : Maybe<TType>.Nothing;
    }
}