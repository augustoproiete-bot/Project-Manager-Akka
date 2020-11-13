using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;

namespace Tauron.Application.Workshop.Core
{
    public abstract class DeferredActor
    {
        private ImmutableList<object>? _stash;
        private Task? _actorTask;

        private IActorRef _actorRef = ActorRefs.Nobody;
        protected IActorRef Actor => _actorRef;

        protected DeferredActor(Task<IActorRef> actor)
        {
            _actorTask = actor.ContinueWith(OnCompleded);
            _stash = ImmutableList<object>.Empty;
        }

        private void OnCompleded(Task<IActorRef> obj)
        {
            lock (this)
            {
                _actorRef = obj.Result;
                foreach (var message in _stash ?? ImmutableList<object>.Empty) 
                    _actorRef.Tell(message);

                _stash = null;
                _actorTask = null;
            }
        }

        protected void TellToActor(object msg)
        {
            if (!Actor.IsNobody())
                Actor.Tell(msg);
            else
            {
                lock (this)
                {
                    if (!Actor.IsNobody())
                    {
                        Actor.Tell(msg);
                        return;
                    }

                    _stash = _stash?.Add(msg) ?? ImmutableList<object>.Empty.Add(msg);
                }
            }
        }
    }
}