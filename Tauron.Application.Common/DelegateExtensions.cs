using System;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron
{
    [PublicAPI]
    public static class DelegateExtensions
    {
        public static TDel Combine<TDel>(this TDel? del1, TDel del2)
            where TDel : Delegate => (TDel)Delegate.Combine(del1, del2);

        public static TDel? Remove<TDel>(this TDel? del1, TDel del2)
            where TDel : Delegate => Delegate.Remove(del1, del2) as TDel;

        public static Maybe<TDel> Combine<TDel>(this Maybe<TDel> mayDel1, Maybe<TDel> mayDel2)
            where TDel : Delegate => Or(from del1 in mayDel1
                                        from del2 in mayDel2
                                        select del1.Combine(del2), mayDel2);


        public static Maybe<TDel> Remove<TDel>(this Maybe<TDel> mayDel1, Maybe<TDel> mayDel2)
            where TDel : Delegate => Collapse(from del1 in mayDel1
                                              from del2 in mayDel2
                                              select MayNotNull(del1.Remove(del2)));
    }
}