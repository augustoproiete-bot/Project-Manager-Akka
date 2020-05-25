using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.MutatingEngine
{
    public sealed class WatchIntrest
    {
        public Action OnRemove { get; }

        public IActorRef Target { get; }

        public WatchIntrest(Action onRemove, IActorRef target)
        {
            OnRemove = onRemove;
            Target = target;
        }
    }
}