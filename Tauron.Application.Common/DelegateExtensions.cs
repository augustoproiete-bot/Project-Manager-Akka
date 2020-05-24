using System;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class DelegateExtensions
    {
        public static TDel? Combine<TDel>(this TDel? del1, TDel? del2)
            where TDel : Delegate =>
            (TDel)Delegate.Combine(del1, del2);

        public sealed class Transform<TSource>
        {
            private readonly Func<TSource> _source;

            public Transform(Func<TSource> source) => _source = source;

            public Func<TNew> To<TNew>(Func<TSource, TNew> transform)
            {
                TNew Func() => transform(_source());

                return Func;
            }
        }

        public static Transform<TSource> From<TSource>(this Func<TSource> source)
            => new Transform<TSource>(source);
    }
}