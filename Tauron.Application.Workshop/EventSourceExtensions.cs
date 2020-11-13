using System;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop
{
    [PublicAPI]
    public static class EventSourceExtensions
    {
        public static void RespondOnEventSource<TData>(this IExposedReceiveActor actor, IEventSource<TData> eventSource, Action<Maybe<TData>> action)
        {
            eventSource.RespondOn(ExposedReceiveActor.ExposedContext.Self);
            actor.Exposed.Receive<Maybe<TData>>((data, _) => action(data));
        }

        public static void RespondOn<TData>(this IEventSource<TData> source, Action<Maybe<TData>> action)
            => source.RespondOn(null, action);

        public static MutateClass<TRecieve, TStart, TMutator> Mutate<TRecieve, TStart, TMutator>(
            this RunSelector<TRecieve, TStart> selector, TMutator mutator) => new MutateClass<TRecieve, TStart, TMutator>(mutator, selector);

        public static EventSourceTargetSelector<TRespond, TStart> EventSource<TRecieve, TStart, TRespond>(this RunSelector<TRecieve, TStart> selector, IEventSource<TRespond> source) 
            => new EventSourceTargetSelector<TRespond, TStart>(selector.Flow, source);

        #region Mutate

        public sealed class MutateReceiveBuilder<TNext, TStart, TRecieve> : ReceiveBuilderBase<TNext, TStart>
        {
            public MutateReceiveBuilder([NotNull] ActorFlowBuilder<TStart> flow, IEventSource<TNext> eventSource, Action<Maybe<TRecieve>> runner, Func<IActorContext, IActorRef> target)
                : base(flow)
            {
                flow.Register(e =>
                {
                    eventSource.RespondOn(target(ExposedReceiveActor.ExposedContext));
                    e.Receive<Maybe<TRecieve>>(new RecieveHelper(runner).Run);
                });
            }

            private sealed class RecieveHelper
            {
                private readonly Action<Maybe<TRecieve>> _runner;

                public RecieveHelper(Action<Maybe<TRecieve>> runner) => _runner = runner;

                public void Run(Maybe<TRecieve> recieve, IActorContext _) => _runner(recieve);
            }
        }

        public sealed class MutateTargetSelector<TNext, TStart, TRecieve> 
            : AbastractTargetSelector<MutateReceiveBuilder<TNext, TStart, TRecieve>, TStart>
        {
            private readonly IEventSource<TNext> _eventSource;
            private readonly Action<Maybe<TRecieve>> _runner;

            public MutateTargetSelector(ActorFlowBuilder<TStart> flow, IEventSource<TNext> eventSource, Action<Maybe<TRecieve>> runner) : base(flow)
            {
                _eventSource = eventSource;
                _runner = runner;
            }

            public override MutateReceiveBuilder<TNext, TStart, TRecieve> ToRef(Func<IActorContext, IActorRef> actorRef) 
                => new MutateReceiveBuilder<TNext, TStart, TRecieve>(Flow, _eventSource, _runner, actorRef);
        }

        [PublicAPI]
        public sealed class MutateClass<TRecieve, TStart, TMutator>
        {
            private readonly TMutator _mutator;
            private readonly RunSelector<TRecieve, TStart> _selector;

            public MutateClass(TMutator mutator, RunSelector<TRecieve, TStart> selector)
            {
                _mutator = mutator;
                _selector = selector;
            }

            public MutateTargetSelector<TNext, TStart, TRecieve> With<TNext>(Func<TMutator, IEventSource<TNext>> eventSource, Func<TMutator, Action<Maybe<TRecieve>>> run) 
                => new MutateTargetSelector<TNext, TStart, TRecieve>(_selector.Flow, eventSource(_mutator), run(_mutator));

            public ActionFinisher<TRecieve, TStart> With(Func<TMutator, Action<TRecieve>> run) 
                => new ActionFinisher<TRecieve, TStart>(_selector.Flow, run(_mutator));
        }

        #endregion

        #region EventSource

        public sealed class EventSourceReceiveBuilder<TEvent, TStart> : ReceiveBuilderBase<TEvent, TStart>
        {
            public EventSourceReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<IActorContext, IActorRef> target, IEventSource<TEvent> evt)
                : base(flow) => flow.Register(_ => evt.RespondOn(target(ExposedReceiveActor.ExposedContext)));
        }

        public sealed class EventSourceTargetSelector<TEvent, TStart> 
            : AbastractTargetSelector<EventSourceReceiveBuilder<TEvent, TStart>, TStart>
        {
            private readonly IEventSource<TEvent> _source;


            public EventSourceTargetSelector(ActorFlowBuilder<TStart> flow, IEventSource<TEvent> source)
                : base(flow) => _source = source;

            public override EventSourceReceiveBuilder<TEvent, TStart> ToRef(Func<IActorContext, IActorRef> actorRef) 
                => new EventSourceReceiveBuilder<TEvent, TStart>(Flow, actorRef, _source);
        }

        #endregion
    }
}