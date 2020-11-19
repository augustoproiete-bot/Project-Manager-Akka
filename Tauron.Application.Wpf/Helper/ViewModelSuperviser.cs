using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.DI.Core;
using Akka.Event;
using Akka.Util;
using Functional.Maybe;
using Tauron.Akka;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Wpf.Helper
{
    public static class ViewModelSuperviserExtensions
    {
        public static void InitModel(this IViewModel model, IUntypedActorContext context, Maybe<string> name = default)
            => ViewModelSuperviser.Get(context.System).Create(model, name);
    }

    public sealed class ViewModelSuperviser
    {
        private static ViewModelSuperviser? _superviser;


        private readonly IActorRef _coordinator;

        private ViewModelSuperviser(IActorRef coordinator) => _coordinator = coordinator;


        public static ViewModelSuperviser Get(ActorSystem system)
            => _superviser ??= new ViewModelSuperviser(system.ActorOf(system.DI().Props<ViewModelSuperviserActor>(), nameof(ViewModelSuperviser)));

        public void Create(IViewModel model, Maybe<string> name = default)
        {
            if (model is ViewModelActorRef actualModel)
                _coordinator.Tell(new CreateModel(actualModel, name));
            else
                throw new InvalidOperationException($"Model mot Compatible with {nameof(ViewModelActorRef)}");
        }

        internal sealed record CreateModel(ViewModelActorRef Model, Maybe<string> Name);
    }

    public sealed class ViewModelSuperviserActor : StatefulReceiveActor<ViewModelSuperviserActor.ViewModelSuperviserState>
    {
        public sealed record ViewModelSuperviserState(int Count);
        
        public ViewModelSuperviserActor()
            : base(new ViewModelSuperviserState(0))
        {
            Receive<ViewModelSuperviser.CreateModel>(NewModel);
        }

        private Maybe<ViewModelSuperviserState> NewModel(ViewModelSuperviser.CreateModel obj, Maybe<ViewModelSuperviserState> mayState)
        {
            var(model, name) = obj;
            
            if (model.IsInitialized) return mayState;

            return from state in mayState
                   let props = Context.System.DI().Props(model.ModelType)
                   let actorName = name.OrElse($"{model.ModelType.Name}--{ObjectState.Count}")
                   let actor = Context.ActorOf(props, actorName)
                   from _ in model.Init(actor)
                   select state with{Count = state.Count + 1};
        }

        protected override SupervisorStrategy SupervisorStrategy()
            => new CircuitBreakerStrategy(Log);

        private sealed class CircuitBreakerStrategy : SupervisorStrategy
        {
            private readonly Func<IDecider> _decider;

            private readonly ConcurrentDictionary<IActorRef, IDecider> _deciders = new();

            private CircuitBreakerStrategy(Func<IDecider> decider)
                => _decider = decider;

            public CircuitBreakerStrategy(ILoggingAdapter log)
                : this(() => new CircuitBreakerDecider(log))
            {
            }

            public override IDecider Decider => throw new NotSupportedException("Single Decider not Supportet");

            protected override Directive Handle(IActorRef child, Exception exception)
            {
                var decider = _deciders.GetOrAdd(child, _ => _decider());
                return decider.Decide(exception);
            }

            public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
            {
                if (restart)
                    RestartChild(child, cause, false);
                else
                    context.Stop(child);
            }

            public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children)
                => _deciders.TryRemove(child, out _);

            public override ISurrogate ToSurrogate(ActorSystem system)
                => throw new NotSupportedException("Can not serialize CircuitBreakerStrategy");
        }

        private sealed class CircuitBreakerDecider : IDecider
        {
            private readonly ILoggingAdapter _log;

            private InternalState _currentState = InternalState.Closed;
            private int           _restartAtempt;

            private int _stateAtempt;

            public CircuitBreakerDecider(ILoggingAdapter log) => _log = log;

            public Directive Decide(Exception cause)
            {
                switch (cause)
                {
                    case ActorInitializationException m:
                        _log.Error(m.InnerException ?? m, "Initialization Error from Model: {Actor}", m.Actor?.Path.Name ?? "Unkowen");
                        return Directive.Escalate;
                    case DeathPactException d:
                        _log.Error(d, "DeathPactException In Model");
                        return Directive.Escalate;
                    case ActorKilledException _:
                        return Directive.Stop;
                }

                _log.Error(cause, "Unhandled Error from Model");

                switch (_currentState)
                {
                    case InternalState.Closed:
                        _stateAtempt  = 1;
                        _currentState = InternalState.HalfOpen;
                        return Directive.Resume;
                    case InternalState.HalfOpen when _stateAtempt > 5:
                        _stateAtempt = 0;
                        _restartAtempt++;

                        if (_restartAtempt > 2)
                            return Directive.Escalate;
                        else
                        {
                            _currentState = InternalState.Closed;
                            return Directive.Restart;
                        }
                    case InternalState.HalfOpen:
                        _stateAtempt++;
                        return Directive.Resume;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private enum InternalState
            {
                Closed,
                HalfOpen
            }
        }
    }
}