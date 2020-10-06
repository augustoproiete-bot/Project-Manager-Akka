using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public abstract class EventSourceBase<TRespond> : DeferredActor, IEventSource<TRespond>
    {
        private Action<TRespond>? _action;
        private ImmutableList<IActorRef> _intrests = ImmutableList<IActorRef>.Empty;
        private ImmutableDictionary<IActorRef, Action<TRespond>> _sourcesActions = ImmutableDictionary<IActorRef, Action<TRespond>>.Empty;

        protected EventSourceBase(Task<IActorRef> mutator)
            : base(mutator)
        {
        }

        public void RespondOn(IActorRef actorRef)
        {
            lock(this)
                Interlocked.Exchange(ref _intrests, _intrests.Add(actorRef));
            TellToActor(new WatchIntrest(() => Interlocked.Exchange(ref _intrests, _intrests.Remove(actorRef)), actorRef));
        }

        public void RespondOn(IActorRef? source, Action<TRespond> action)
        {
            if (source.IsNobody())
            {
                if (_action == null)
                    _action = action;
                else
                    _action += action;
            }
            else
            {
                ImmutableInterlocked.AddOrUpdate(ref _sourcesActions!, source, _ => action, (_, old) => old.Combine(action) ?? action);
                TellToActor(new WatchIntrest(() => ImmutableInterlocked.TryRemove(ref _sourcesActions!, source, out _), source!));
            }
        }

        protected void Send(TRespond respond)
        {
            _intrests.ForEach(ar => ar.Tell(respond));
            _action?.Invoke(respond);
            _sourcesActions.Foreach(p => p.Key.Tell(IncommingEvent.From(respond, p.Value)));
        }
    }
}