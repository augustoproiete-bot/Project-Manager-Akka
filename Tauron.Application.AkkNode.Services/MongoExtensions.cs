using System;
using System.Linq;
using MongoDB.Driver;

namespace Tauron.Application.AkkNode.Services
{
    public static class MongoExtensions
    {
        public static bool Contains<TSource>(this IAsyncCursor<TSource> cursor, Func<TSource, bool> predicate)
        {
            while (cursor.MoveNext())
            {
                if (cursor.Current.Any(predicate))
                    return true;
            }

            return false;
        }
    }
}