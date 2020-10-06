using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class IncommingEvent
    {
        public Action Action { get; }

        private IncommingEvent(Action action) => Action = action;

        public static IncommingEvent From<TData>(TData data, Action<TData> dataAction)
            => new IncommingEvent(() => dataAction(data));
    }

    public interface IEventSource<out TRespond>
    {
        void RespondOn(IActorRef actorRef);

        void RespondOn(IActorRef? source, Action<TRespond> action);
    }
}