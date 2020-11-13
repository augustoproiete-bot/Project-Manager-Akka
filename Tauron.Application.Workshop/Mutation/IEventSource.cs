using System;
using Akka.Actor;
using Akka.Event;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class IncommingEvent : IDeadLetterSuppression
    {
        public Action Action { get; }

        private IncommingEvent(Action action) => Action = action;

        public static IncommingEvent From<TData>(Maybe<TData> data, Action<Maybe<TData>> dataAction)
            => new IncommingEvent(() => dataAction(data));
    }

    public interface IEventSource<TRespond>
    {
        void RespondOn(IActorRef actorRef);

        void RespondOn(IActorRef? source, Action<Maybe<TRespond>> action);
    }
}