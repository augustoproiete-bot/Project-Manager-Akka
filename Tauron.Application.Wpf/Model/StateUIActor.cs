using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Operations;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public abstract class StateUIActor : UiActor
    {
        private readonly Dictionary<Type, Action<IOperationResult>> _compledActions = new Dictionary<Type, Action<IOperationResult>>();

        public IActionInvoker ActionInvoker { get; }

        protected StateUIActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IActionInvoker actionInvoker) : base(lifetimeScope, dispatcher)
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
            private readonly StateUIActor _actor;

            public UIStateConfiguration(TState state, StateUIActor actor)
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
            private readonly StateUIActor _actor;

            public UIStateEventConfiguration(IEventSource<TEvent> eventSource, StateUIActor actor)
            {
                _eventSource = eventSource;
                _actor = actor;
            }

            public FluentPropertyRegistration<TData> ToProperty<TData>(string name, Func<TEvent, TData> transform, Func<TEvent, bool>? condition = null)
            {
                var propertyConfig = _actor.RegisterProperty<TData>(name);
                var property = propertyConfig.Property;

                _eventSource.RespondOn(_actor.Self, evt =>
                {
                    if (condition != null && !condition(evt))
                        return;

                    property.Set(transform(evt));
                });

                return propertyConfig;
            }
        }
    }

    [PublicAPI]
    public static class StateUIActorExtenstions
    {
        public static CommandRegistrationBuilder ToStateAction<TStateAction>(this CommandRegistrationBuilder builder)
            where TStateAction : IStateAction, new() 
            => ToStateAction(builder, _ => new TStateAction());

        public static CommandRegistrationBuilder ToStateAction(this CommandRegistrationBuilder builder, Func<IStateAction> action) 
            => ToStateAction(builder, _ => action());

        public static CommandRegistrationBuilder ToStateAction<TParameter>(this CommandRegistrationBuilder builder, Func<TParameter, IStateAction> action)
        {
            return ToStateAction(builder, o =>
            {
                if (o is TParameter parameter)
                    return action(parameter);

                return action(default!);
            });
        }

        public static CommandRegistrationBuilder ToStateAction(this CommandRegistrationBuilder builder, Func<object?, IStateAction> action)
        {
            var invoker = TryCast(builder);

            return builder.WithExecute(o => invoker.DispatchAction(action(o)));
        }

        private static StateUIActor TryCast(CommandRegistrationBuilder builder)
        {
            if (builder.Target is StateUIActor uiActor)
                return uiActor;

            throw new InvalidOperationException("command Builder is not a State Actor");
        }
    }
}