using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class WatchIntrest
    {
        public WatchIntrest(Action onRemove, IActorRef target)
        {
            OnRemove = onRemove;
            Target = target;
        }

        public Action OnRemove { get; }

        public IActorRef Target { get; }
    }
}