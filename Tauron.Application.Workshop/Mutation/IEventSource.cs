using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IEventSource<TRespond>
    {
        void RespondOn(IActorRef actorRef);

        void RespondOn(Action<TRespond> action);
    }
}