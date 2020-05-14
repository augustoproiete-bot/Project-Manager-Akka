using System;
using Akka.Actor;

namespace Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine
{
    public interface IEventSource<TRespond>
    {
        void RespondOn(IActorRef actorRef);

        void RespondOn(Action<TRespond> action);
    }
}