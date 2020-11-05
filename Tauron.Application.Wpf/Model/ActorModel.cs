using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Operations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public abstract class ActorModel : ExposedReceiveActor
    {
        private readonly Dictionary<Type, Action<IOperationResult>> _compledActions = new Dictionary<Type, Action<IOperationResult>>();

        public IActionInvoker ActionInvoker { get; }

        protected ActorModel(IActionInvoker actionInvoker)
        {
            ActionInvoker = actionInvoker;
            Receive<IOperationResult>(OnOperationCompled);
        }

        protected virtual void OnOperationCompled(IOperationResult result)
        {
            var actionType = result.Outcome?.GetType();

            if (actionType?.IsAssignableTo<IStateAction>() == true)
            {
                if (_compledActions.TryGetValue(actionType, out var action))
                    action(result);
            }
        }

        public void WhenActionComnpled<TAction>(Action<IOperationResult> opsAction)
            where TAction : IStateAction
        {
            var key = typeof(TAction);
            _compledActions[key] = opsAction.Combine(_compledActions.GetValueOrDefault(key))!;
        }

        public UIStateConfiguration<TState> WhenStateChanges<TState>(string? name = null)
            where TState : class => new UIStateConfiguration<TState>(ActionInvoker.GetState<TState>(name ?? string.Empty) ?? throw new ArgumentException("No such State Found"), this);

        public void DispatchAction(IStateAction action, bool? sendBack = true)
            => ActionInvoker.Run(action, sendBack);

        [PublicAPI]
        public sealed class UIStateConfiguration<TState>
        {
            private readonly TState _state;
            private readonly ActorModel _actor;

            public UIStateConfiguration(TState state, ActorModel actor)
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
            private readonly IEventSource<TEvent> _eventSource;
            private readonly ActorModel _actor;

            public UIStateEventConfiguration(IEventSource<TEvent> eventSource, ActorModel actor)
            {
                _eventSource = eventSource;
                _actor = actor;
            }

            public ActorFlowBuilder<TEvent> ToFlow()
            {
                _eventSource.RespondOn(_actor.Self);
                return new ActorFlowBuilder<TEvent>(_actor);
            }
        }
    }
}