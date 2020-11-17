using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;
using Serilog;
using static Tauron.Preload;

namespace Tauron.Application.Workshop.Core
{
    public abstract class DeferredActor<TState> : StatefulObject<TState>
        where TState : DeferredActor<TState>.DeferredActorState
    {
        public record DeferredActorState(ImmutableList<object>? Stash, IActorRef Actor);

        protected IActorRef Actor => ObjectState.Actor;

        protected DeferredActor(Task<IActorRef> actor, TState initialState) 
            : base(initialState, true)
            => actor.ContinueWith(OnCompleded);

        private void OnCompleded(Task<IActorRef> obj)
        {
            static Maybe<Unit> TellAll(IActorRef actor, IEnumerable<object> messages)
            {
                foreach (var message in messages) actor.Tell(message);
                return Unit.MayInstance;
            }

            try
            {
                Run(s =>
                    from state in s
                    let actor = obj.Result
                    from stash in MayNotNull(state.Stash) 
                    from _ in TellAll(actor, stash) 
                    select state with{Stash = null, Actor = actor});
            }
            catch (Exception e)
            {
                Log.Logger.ForContext(GetType()).Error(e, "Error on Initializing Actor");
            }
        }

        protected void TellToActor(object msg)
        {
            static Maybe<bool> TryTell(IActorRef mayActor, object msg)
            {
                var tell =
                    from actor in MayActor(mayActor)
                    select actor;

                return Match(tell,
                    actor =>
                    {
                        actor.Tell(msg);
                        return true;
                    },
                    () => May(false));
            }

            if (!Actor.IsNobody())
                Actor.Tell(msg);
            else
            {
                Run(s =>
                    from state in s
                    from result in TryTell(state.Actor, msg)
                    where !result
                    from stash in MayNotNull(state.Stash)
                    select state with{Stash = stash.Add(msg)}
                    );
            }
        }
    }
}