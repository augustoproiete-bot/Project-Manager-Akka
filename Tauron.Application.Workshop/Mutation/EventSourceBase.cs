using System;
using System.Collections.Immutable;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public abstract class EventSourceBase<TRespond> : IEventSource<TRespond>
    {
        private readonly IActorRef _mutator;
        private Action<TRespond>? _action;
        private ImmutableList<IActorRef> _intrests = ImmutableList<IActorRef>.Empty;

        protected EventSourceBase(IActorRef mutator)
        {
            _mutator = mutator;
        }

        public void RespondOn(IActorRef actorRef)
        {
            Interlocked.Exchange(ref _intrests, _intrests.Add(actorRef));
            _mutator.Tell(new WatchIntrest(() => Interlocked.Exchange(ref _intrests, _intrests.Remove(actorRef)), actorRef));
        }

        public void RespondOn(Action<TRespond> action)
        {
            if (_action == null)
                _action = action;
            else
                _action += action;
        }

        protected void Send(TRespond respond)
        {
            _intrests.ForEach(ar => ar.Tell(respond));
            _action?.Invoke(respond);
        }
    }
}