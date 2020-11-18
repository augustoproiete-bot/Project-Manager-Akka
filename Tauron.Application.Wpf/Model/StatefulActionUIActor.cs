using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Operations;

namespace Tauron.Application.Wpf.Model
{
    public abstract class ActionUIActor : ActionUIActor<EmptyState>
    {
        protected ActionUIActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IActionInvoker actionInvoker) 
            : base(lifetimeScope, dispatcher, actionInvoker)
        {
        }
    }

    public interface IActionUIActor : IUiActor
    {
        IActionInvoker ActionInvoker { get; }

        void DispatchAction(IStateAction action, bool? sendBack = true);
    }

    [PublicAPI]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public abstract class ActionUIActor<TState> : StatefulUiActor<TState>, IActionUIActor where TState : new()
    {
        private readonly Dictionary<Type, Action<IOperationResult>> _compledActions = new();

        protected ActionUIActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IActionInvoker actionInvoker) : base(lifetimeScope, dispatcher)
        {
            ActionInvoker = actionInvoker;
            Receive<IOperationResult>(OnOperationCompled);
        }

        public IActionInvoker ActionInvoker { get; }

        protected virtual void OnOperationCompled(IOperationResult result)
        {
            var actionType = result.Outcome?.GetType();

            if (actionType?.IsAssignableTo<IStateAction>() == true)
            {
                if (_compledActions.TryGetValue(actionType, out var action))
                    action(result);
            }
        }

        public TActionState GetState<TActionState>(Maybe<string> key = default) where TActionState : class
            => ActionInvoker.GetState<TActionState>(key).OrElse(() => new InvalidOperationException("No such State Found"));

        public void WhenActionComnpled<TAction>(Action<IOperationResult> opsAction)
            where TAction : IStateAction
        {
            var key = typeof(TAction);
            _compledActions[key] = _compledActions.GetValueOrDefault(key).Combine(opsAction);
        }

        public UIStateConfiguration<TActionState> WhenStateChanges<TActionState>(Maybe<string> name = default)
            where TActionState : class => new(ActionInvoker.GetState<TActionState>(name).OrElse(() => new ArgumentException("No such State Found")), this);

        public void DispatchAction(IStateAction action, bool? sendBack = true)
            => ActionInvoker.Run(action, sendBack);

        [PublicAPI]
        public sealed class UIStateConfiguration<TActionState>
        {
            private readonly ActionUIActor<TState> _actor;
            private readonly TActionState       _state;

            public UIStateConfiguration(TActionState state, ActionUIActor<TState> actor)
            {
                _state = state;
                _actor = actor;
            }

            public UIStateEventConfiguration<TEvent> FromEvent<TEvent>(Func<TActionState, IEventSource<TEvent>> source, Action<UIStateEventConfiguration<TEvent>>? configAction = null)
            {
                var config = new UIStateEventConfiguration<TEvent>(source(_state), _actor);
                configAction?.Invoke(config);
                return config;
            }
        }

        [PublicAPI]
        public sealed class UIStateEventConfiguration<TEvent>
        {
            private readonly ActionUIActor<TState> _actor;
            private readonly IEventSource<TEvent> _eventSource;

            public UIStateEventConfiguration(IEventSource<TEvent> eventSource, ActionUIActor<TState> actor)
            {
                _eventSource = eventSource;
                _actor       = actor;
            }

            public FluentPropertyRegistration<TData> ToProperty<TData>(string name, Func<Maybe<TEvent>, Maybe<TData>> transform, Func<Maybe<TEvent>, bool>? condition = null)
            {
                var propertyConfig = _actor.RegisterProperty<TData>(name);
                var property       = propertyConfig.Property;

                _eventSource.RespondOn(_actor.Self, evt =>
                                                    {
                                                        if (condition != null && !condition(evt))
                                                            return;

                                                        property.Set(transform(evt)!);
                                                    });

                return propertyConfig;
            }

            public void ToAction(Action<TEvent> action)
            {
                _eventSource.RespondOn(_actor.Self);
                _actor.Receive(action);
            }
        }
    }

    [PublicAPI]
    public static class StateUIActorExtenstions
    {
        public static CommandRegistrationBuilder ToStateAction<TStateAction>(this CommandRegistrationBuilder builder)
            where TStateAction : IStateAction, new()
            => ToStateAction(builder, _ => new TStateAction());

        public static CommandRegistrationBuilder ToStateAction(this CommandRegistrationBuilder builder, Func<IStateAction?> action)
            => ToStateAction(builder, _ => action());

        public static CommandRegistrationBuilder ToStateAction<TParameter>(this CommandRegistrationBuilder builder, Func<TParameter, IStateAction?> action)
        {
            return ToStateAction(builder, o =>
                                          {
                                              if (o is TParameter parameter)
                                                  return action(parameter);

                                              return action(default!);
                                          });
        }

        public static CommandRegistrationBuilder ToStateAction(this CommandRegistrationBuilder builder, Func<object?, IStateAction?> action)
        {
            var invoker = TryCast(builder);

            return builder.WithExecute(o =>
                                       {
                                           var stateAction = action(o);
                                           if (stateAction == null) return;

                                           invoker.DispatchAction(stateAction);
                                       });
        }

        private static IActionUIActor TryCast(CommandRegistrationBuilder builder)
        {
            if (builder.Target is IActionUIActor uiActor)
                return uiActor;

            throw new InvalidOperationException("command Builder is not a State Actor");
        }
    }
}