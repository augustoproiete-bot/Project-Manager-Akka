using System;
using System.Collections.Immutable;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Operations;

namespace Tauron.Application.Wpf.Model
{
    public abstract class ActorModel : StatefulActorModel<ActorModel.ActorModelState>
    {
        public record ActorModelState(ImmutableDictionary<Type, Action<IOperationResult>> CompledActions);

        protected ActorModel(IActionInvoker actionInvoker) 
            : base(actionInvoker, new ActorModelState(ImmutableDictionary<Type, Action<IOperationResult>>.Empty))
        {
        }
    }
    
    [PublicAPI]
    public abstract class StatefulActorModel<TModelState> : StatefulReceiveActor<TModelState>
        where TModelState : ActorModel.ActorModelState
    {
        protected StatefulActorModel(IActionInvoker actionInvoker, TModelState state)
            : base(state)
        {
            ActionInvoker = actionInvoker;
            
            Receive<IncommingEvent>(evt => evt.Action());
            Receive<IOperationResult>(OnOperationCompled);
        }

        public IActionInvoker ActionInvoker { get; }

        protected virtual void OnOperationCompled(IOperationResult result)
        {
            var actionType = result.Outcome?.GetType();

            if (actionType?.IsAssignableTo<IStateAction>() != true) return;
            
            if (ObjectState.CompledActions.TryGetValue(actionType, out var action))
                action(result);
        }

        public void WhenActionComnpled<TAction>(Action<IOperationResult> opsAction)
            where TAction : IStateAction
        {
            Run(s =>
                    from state in s
                    let key = typeof(TAction)
                    let action = state.CompledActions.GetValueOrDefault(key).Combine(opsAction)
                    select state with{CompledActions = state.CompledActions.SetItem(key, action)});
        }

        public UIStateConfiguration<TState> WhenStateChanges<TState>(Maybe<string> name = default)
            where TState : class => new(ActionInvoker.GetState<TState>(name).OrElse(() => new ArgumentException("No such State Found")), this);

        public void DispatchAction(IStateAction action, bool? sendBack = true)
            => ActionInvoker.Run(action, sendBack);

        [PublicAPI]
        public sealed class UIStateConfiguration<TState>
        {
            private readonly StatefulActorModel<TModelState> _actor;
            private readonly TState                          _state;

            public UIStateConfiguration(TState state, StatefulActorModel<TModelState> actor)
            {
                _state = state;
                _actor = actor;
            }

            public UIStateEventConfiguration<TEvent> FromEvent<TEvent>(Func<TState, IEventSource<TEvent>> source, Action<UIStateEventConfiguration<TEvent>>? configAction = null)
            {
                var config = new UIStateEventConfiguration<TEvent>(source(_state), _actor);
                configAction?.Invoke(config);
                return config;
            }
        }

        [PublicAPI]
        public sealed class UIStateEventConfiguration<TEvent>
        {
            private readonly StatefulActorModel<TModelState> _actor;
            private readonly IEventSource<TEvent>            _eventSource;

            public UIStateEventConfiguration(IEventSource<TEvent> eventSource, StatefulActorModel<TModelState> actor)
            {
                _eventSource = eventSource;
                _actor       = actor;
            }

            public ActorFlowBuilder<TEvent> ToFlow()
            {
                _eventSource.RespondOn(_actor.Self);
                return new ActorFlowBuilder<TEvent>(_actor);
            }
        }
    }
}