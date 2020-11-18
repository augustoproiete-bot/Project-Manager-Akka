using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util.Internal;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Core;
using static Tauron.Prelude;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public abstract class EventSourceBase<TRespond> : DeferredActor<EventSourceBase<TRespond>.EventSourceState>, IEventSource<TRespond>
    {
        public sealed record EventSourceState(ImmutableList<object>? Stash, IActorRef Actor, Action<Maybe<TRespond>>? Action, ImmutableList<IActorRef> Intrests, 
                                              ImmutableDictionary<IActorRef, Action<Maybe<TRespond>>> SourceActions)
            : DeferredActorState(Stash, Actor)
        {
            public EventSourceState()
                : this(ImmutableList<object>.Empty, ActorRefs.Nobody, null, ImmutableList<IActorRef>.Empty, ImmutableDictionary<IActorRef, Action<Maybe<TRespond>>>.Empty)
            {
                
            }
        }
        
        
        private readonly WorkspaceSuperviser _superviser;

        protected EventSourceBase(Task<IActorRef> mutator, WorkspaceSuperviser superviser)
            : base(mutator, new EventSourceState()) 
            => _superviser = superviser;
        
        public void RespondOn(IActorRef actorRef)
        {
            WatchIntrest CreateIntrest()
            {
                return new(actorRef,
                           () => Run(s =>
                                         from state in s
                                         select state with{Intrests = state.Intrests.Remove(actorRef)}));
            }
            
            Run(s =>
                    from state in s
                    from _ in MayUse(() => _superviser.WatchIntrest(CreateIntrest()))
                    select state with{Intrests = state.Intrests.Add(actorRef)});
        }

        public void RespondOn(IActorRef? source, Action<Maybe<TRespond>> action)
        {
            WatchIntrest CreateIntrest(IActorRef actor)
            {
                return new(actor,
                           () => Run(s =>
                                         from state in s
                                         select state with{SourceActions = state.SourceActions.Remove(actor)}));
            }
            
            if (source == null || source.IsNobody())
            {
                Run(s =>
                        from state in s 
                        select state with{Action = state.Action.Combine(action)});
            }
            else
            {
                Run(s =>
                        from state in s
                        from _ in MayUse(() => _superviser.WatchIntrest(CreateIntrest(source)))
                        let list = state.SourceActions
                        select state with{ SourceActions = list.ContainsKey(source)
                                                               ? list.SetItem(source, list[source].Combine(action))
                                                               : list.SetItem(source, action)});
            }
        }

        protected void Send(Maybe<TRespond> respond)
        {
            if(respond.IsNothing()) return;

            var current = ObjectState;
            
            current.Intrests.ForEach(ar => ar.Tell(respond));
            current.Action?.Invoke(respond);
            current.SourceActions.ForEach(p => p.Key.Tell(IncommingEvent.From(respond, p.Value)));
        }
    }
}