using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public static class EventSourceExtensions
    {
        #region Mutate

        public sealed class MutateReceiveBuilder<TNext, TStart, TParent, TRecieve> : ReceiveBuilderBase<TNext, TStart, TParent>
        {
            private sealed class RecieveHelper
            {
                private readonly Action<TRecieve> _runner;

                public RecieveHelper(Action<TRecieve> runner) => _runner = runner;

                public void Run(TRecieve recieve, IActorContext context)
                    => _runner(recieve);
            }

            public MutateReceiveBuilder([NotNull] ActorFlowBuilder<TStart, TParent> flow, IEventSource<TNext> eventSource, Action<TRecieve> runner, Func<IActorContext, IActorRef> target) 
                : base(flow)
            {
                flow.Register(e =>
                              {
                                  eventSource.RespondOn(target(Flow.Actor.ExposedContext));
                                  e.Exposed.Receive<TRecieve>(new RecieveHelper(runner).Run);
                              });
            }
        }

        public sealed class MutateTargetSelector<TNext, TStart, TParent, TRecieve> : AbastractTargetSelector<MutateReceiveBuilder<TNext, TStart, TParent, TRecieve>, TStart, TParent>
        {
            private readonly IEventSource<TNext> _eventSource;
            private readonly Action<TRecieve> _runner;

            public MutateTargetSelector(ActorFlowBuilder<TStart, TParent> flow, IEventSource<TNext> eventSource, Action<TRecieve> runner) : base(flow)
            {
                _eventSource = eventSource;
                _runner = runner;
            }

            public override MutateReceiveBuilder<TNext, TStart, TParent, TRecieve> ToRef(Func<IActorContext, IActorRef> actorRef) 
                => new MutateReceiveBuilder<TNext, TStart, TParent, TRecieve>(Flow, _eventSource, _runner, actorRef);
        }

        public sealed class MutateClass<TRecieve, TStart, TParent, TMutator>
        {
            private readonly TMutator _mutator;
            private readonly RunSelector<TRecieve, TStart, TParent> _selector;

            public MutateClass(TMutator mutator, RunSelector<TRecieve, TStart, TParent> selector)
            {
                _mutator = mutator;
                _selector = selector;
            }

            public MutateTargetSelector<TNext, TStart, TParent, TRecieve> For<TNext>(
                Func<TMutator, IEventSource<TNext>> eventSource, Func<TMutator, Action<TRecieve>> run) =>
                new MutateTargetSelector<TNext, TStart, TParent, TRecieve>(_selector.Flow, eventSource(_mutator), run(_mutator));
            public ActionFinisher<TRecieve, TStart, TParent> For(Func<TMutator, Action<TRecieve>> run) =>
                new ActionFinisher<TRecieve, TStart, TParent>(_selector.Flow, run(_mutator));
        }

        #endregion

        #region EventSource

        public sealed class EventSourceReceiveBuilder<TEvent, TStart, TParent> : ReceiveBuilderBase<TEvent, TStart, TParent>
        {
            public EventSourceReceiveBuilder(ActorFlowBuilder<TStart, TParent> flow, Func<IActorContext, IActorRef> target, IEventSource<TEvent> evt) 
                : base(flow) =>
                flow.Register(e => evt.RespondOn(target(flow.Actor.ExposedContext)));
        }

        public sealed class EventSourceTargetSelector<TEvent, TStart, TParent> : AbastractTargetSelector<EventSourceReceiveBuilder<TEvent, TStart, TParent>, TStart, TParent>
        {
            private readonly IEventSource<TEvent> _source;

            public EventSourceTargetSelector(ActorFlowBuilder<TStart, TParent> flow, IEventSource<TEvent> source) 
                : base(flow) =>
                _source = source;

            public override EventSourceReceiveBuilder<TEvent, TStart, TParent> ToRef(Func<IActorContext, IActorRef> actorRef) 
                => new EventSourceReceiveBuilder<TEvent, TStart, TParent>(Flow, actorRef, _source);
        }

        #endregion

        public static void RespondOnEventSource<TData>(this ExposedReceiveActor actor, IEventSource<TData> eventSource, Action<TData> action)
        {
            eventSource.RespondOn(actor.ExposedContext.Self);
            actor.Exposed.Receive<TData>((data, context) => action(data));
        }

        public static MutateClass<TRecieve, TStart, TParent, TMutator> Mutate<TRecieve, TStart, TParent, TMutator>(
            this RunSelector<TRecieve, TStart, TParent> selector, TMutator mutator) =>
            new MutateClass<TRecieve, TStart,TParent,TMutator>(mutator, selector);

        public static EventSourceTargetSelector<TRespond, TStart, TParent> EventSource<TRecieve, TStart, TParent, TRespond>(this RunSelector<TRecieve, TStart, TParent> selector, IEventSource<TRespond> source) 
            => new EventSourceTargetSelector<TRespond, TStart, TParent>(selector.Flow, source);
    }
}
