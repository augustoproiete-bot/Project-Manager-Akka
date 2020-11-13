using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util.Internal;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public abstract class EventSourceBase<TRespond> : DeferredActor, IEventSource<TRespond>
    {
        private readonly WorkspaceSuperviser _superviser;

        private Action<Maybe<TRespond>>? _action;
        private ImmutableList<IActorRef> _intrests = ImmutableList<IActorRef>.Empty;
        private ImmutableDictionary<IActorRef, Action<Maybe<TRespond>>> _sourcesActions = ImmutableDictionary<IActorRef, Action<Maybe<TRespond>>>.Empty;

        protected EventSourceBase(Task<IActorRef> mutator, WorkspaceSuperviser superviser)
            : base(mutator) 
            => _superviser = superviser;

        public void RespondOn(IActorRef actorRef)
        {
            lock(this)
                Interlocked.Exchange(ref _intrests, _intrests.Add(actorRef));
            _superviser.WatchIntrest(new WatchIntrest(() => Interlocked.Exchange(ref _intrests, _intrests.Remove(actorRef)), actorRef));
        }

        public void RespondOn(IActorRef? source, Action<Maybe<TRespond>> action)
        {
            if (source.IsNobody())
                _action = _action.Combine(action);
            else
            {
                ImmutableInterlocked.AddOrUpdate(ref _sourcesActions!, source, _ => action, (_, old) => old.Combine(action) ?? action);
                _superviser.WatchIntrest(new WatchIntrest(() => ImmutableInterlocked.TryRemove(ref _sourcesActions!, source, out _), source!));
            }
        }

        protected void Send(Maybe<TRespond> respond)
        {
            if(respond.IsNothing()) return;

            _intrests.ForEach(ar => ar.Tell(respond));
            _action?.Invoke(respond);
            _sourcesActions.ForEach(p => p.Key.Tell(IncommingEvent.From(respond, p.Value)));
        }
    }
}