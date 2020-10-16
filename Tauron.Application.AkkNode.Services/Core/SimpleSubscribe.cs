using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.Core
{
    [PublicAPI]
    public static class SimpleSubscribeFlow
    {
        public sealed class EventRecieve<TNew, TStart> : ReceiveBuilderBase<TNew, TStart>
        { 
            public EventRecieve(ActorFlowBuilder<TStart> flow, IActorRef target) 
                : base(flow) =>
                flow.Register(ad => target.Tell(new EventSubscribe(typeof(TNew))));
        }

        //public sealed class EventSelector<TRecieve, TStart, TParent>
        //{
        //    private readonly RunSelector<TRecieve, TStart, TParent> _selector;

        //    public EventSelector(RunSelector<TRecieve, TStart, TParent> selector) => _selector = selector;

        //    public EventRecieve<TNew, TStart, TParent> For<TNew>(IActorRef target)
        //        => new EventRecieve<TNew, TStart, TParent>(_selector.Flow, target);
        //}

        public static EventRecieve<TRecieve, TStart> Event<TRecieve, TStart>(this RunSelector<TRecieve, TStart> selector, IActorRef target)
            => new EventRecieve<TRecieve, TStart>(selector.Flow, target);
    }

    [PublicAPI]
    public static class SimpleSubscribe
    {
        public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target, bool killOnFirstResponse = false)
        {
            var eventActor = EventActor.Create(actor, null, killOnFirstResponse);
            eventActor.Send(target, new EventSubscribe(typeof(TEvent)));
            return eventActor;
        }

        public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target, Action<TEvent> handler, bool killOnFirstResponse = false)
        {
            var eventActor = EventActor.Create(actor, handler, killOnFirstResponse);
            eventActor.Send(target, new EventSubscribe(typeof(TEvent)));
            return eventActor;
        }

        public static EventSubscribtion SubscribeToEvent<TEvent>(this IActorRef eventSource)
        {
            eventSource.Tell(new EventSubscribe(typeof(TEvent)));
            return new EventSubscribtion(typeof(TEvent), eventSource);
        }
    }

    [PublicAPI]
    public sealed class EventSubscribtion : IDisposable
    {
        public static EventSubscribtion Empty { get; } = new EventSubscribtion(typeof(Type), ActorRefs.Nobody);

        private readonly Type _event;
        private readonly IActorRef _eventSource;

        public EventSubscribtion(Type @event, IActorRef eventSource)
        {
            _event = @event;
            _eventSource = eventSource;
        }

        public void Dispose()
        {
            if(_eventSource.IsNobody()) return;
            _eventSource.Tell(new EventUnSubscribe(_event));
        }
    }
}