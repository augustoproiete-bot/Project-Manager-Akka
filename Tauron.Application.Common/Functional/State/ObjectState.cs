using System;
using Functional.Maybe;
using static Tauron.Preload;

namespace Tauron
{
    public record ObjectState<TState>(TState Data)
    {
        public ObjectState<TState> Run(Func<Maybe<TState>, Maybe<TState>> operation)
        {
            return OrElse(from data in operation(May(Data))
                          select this with{ Data = data }, this);
        }
    }

    public record LockedObjectState<TState>(object Lock, TState Data) : ObjectState<TState>(Data)
    {
    }
}
