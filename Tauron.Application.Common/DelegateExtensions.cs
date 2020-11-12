using System;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class DelegateExtensions
    {
        public static TDel? Combine<TDel>(this TDel? del1, TDel? del2)
            where TDel : Delegate => Delegate.Combine(del1, del2) as TDel;

        public static TDel? Remove<TDel>(this TDel? del1, TDel del2)
            where TDel : Delegate => Delegate.Remove(del1, del2) as TDel;

        public static Transform<TSource> From<TSource>(this Func<TSource> source) => new(source);

        public sealed class Transform<TSource>
        {
            private readonly Func<TSource> _source;

            public Transform(Func<TSource> source) => _source = source;

            public Func<TNew> To<TNew>(Func<TSource, TNew> transform)
            {
                TNew Func()
                {
                    return transform(_source());
                }

                return Func;
            }
        }
    }
}