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
    }
}