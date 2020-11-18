using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Util;
using Akka.Util.Internal;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.Model
{
    public abstract class UiActor : StatefulUiActor<EmptyState>
    {
        protected UiActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
        }
    }

    public interface IUiActor : IExposedReceiveActor
    {
        ILifetimeScope LifetimeScope { get; }
        Dispatcher Dispatcher { get; }
        void InvalidateRequerySuggested();
        void Flow<TStart>(Action<ActorFlowBuilder<TStart>> builder);
        EnterFlow<TStart> EnterFlow<TStart>(Action<ActorFlowBuilder<TStart>> builder);

        FluentPropertyRegistration<TData> RegisterProperty<TData>(string name);

        void ThrowIsSeald();

        void RegisterProperty(UIPropertyBase prop);

        void RegisterTerminationCallback(Action<IUiActor> callback);
    }

    [PublicAPI]
    public abstract class StatefulUiActor<TState> : StatefulReceiveActor<TState>, IUiActor where TState : new()
    {
        private sealed record UiActorData(ImmutableDictionary<string, CommandRegistration> CommandRegistrations, ImmutableGroupDictionary<string, InvokeHelper> EventRegistrations,
            ImmutableDictionary<string, PropertyData> Propertys, bool IsSealed);

        private readonly StatefulObjectLogic<UiActorData> _actorData;

        protected StatefulUiActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher)
            : base(new TState())
        {
            _actorData = new StatefulObjectLogic<UiActorData>(new UiActorData(
                ImmutableDictionary<string, CommandRegistration>.Empty,
                ImmutableGroupDictionary<string, InvokeHelper>.Empty,
                ImmutableDictionary<string, PropertyData>.Empty, false));

            LifetimeScope = lifetimeScope;
            Dispatcher    = dispatcher;
            InitHandler();
        }

        //internal IUntypedActorContext UIActorContext => Context;

        public ILifetimeScope LifetimeScope { get; }

        public override void AroundPreStart()
        {
            _actorData.Run(d =>
                from state in d
                select state with{IsSealed = true});
            base.AroundPreStart();
        }

        public void ThrowIsSeald()
        {
            if (_actorData.ObjectState.IsSealed)
                throw new InvalidOperationException("The Ui Actor is immutale");
        }

        protected void InitHandler()
        {
            Receive<CanCommandExecuteRequest>(CanCommandExecute);
            Receive<CommandExecuteEvent>(CommandExecute);
            Receive<ControlSetEvent>(msg => SetControl(msg.Name, msg.Element));
            Receive<MakeEventHook>(msg => Context.Sender.Tell(Context.GetOrCreateEventActor(May(msg.Name + "-EventActor"))));
            Receive<ExecuteEventExent>(ExecuteEvent);
            Receive<GetValueRequest>(GetPropertyValue);
            ReceiveAsync<InitEvent>(async evt => await InitializeAsync(evt));
            Receive<SetValue>(SetPropertyValue);
            Receive<TrackPropertyEvent>(t => TrackProperty(t, Sender));
            Receive<Terminated>(ActorTermination);
            Receive<PropertyTermination>(PropertyTerminationHandler);
            Receive<UnloadEvent>(ControlUnload);
            Receive<InitParentViewModel>(InitParentViewModel);
            Receive<ReviveActor>(RestartActor);
        }

        #region ControlEvents

        protected virtual void SetControl(string name, FrameworkElement element)
        {
        }

        #endregion

        private sealed record CommandRegistration(Action<Maybe<object>> Command, Maybe<CommandQuery> CanExecute);

        private sealed record PropertyTermination(IActorRef ActorRef, string Name);

        private sealed class InvokeHelper
        {
            private readonly Delegate   _method;
            private readonly MethodType _methodType;

            public InvokeHelper(Delegate del)
            {
                _method = del;
                var method = del.Method;

                _methodType = (MethodType) method.GetParameters().Length;
                if (_methodType                             != MethodType.One) return;
                if (method.GetParameters()[0].ParameterType != typeof(EventData)) _methodType = MethodType.EventArgs;
            }

            public void Execute(EventData? parameter)
            {
                var args = _methodType switch
                {
                    MethodType.Zero => new object[0],
                    MethodType.One => new object[] {parameter!},
                    MethodType.Two => new[] {parameter?.Sender, parameter?.EventArgs},
                    MethodType.EventArgs => new[] {parameter?.EventArgs},
                    _ => new object[0]
                };

                _method.Method.InvokeFast(_method.Target, args);
            }

            private enum MethodType
            {
                Zero = 0,
                One,
                Two,
                EventArgs
            }
        }

        private sealed class PropertyData
        {
            public PropertyData(UIPropertyBase propertyBase) => PropertyBase = propertyBase;

            public UIPropertyBase PropertyBase { get; }

            public Maybe<string> Error { get; set; }

            public List<IActorRef> Subscriptors { get; } = new();

            public void SetValue(Maybe<object?> value)
            {
                if(value.IsSomething())
                    PropertyBase.SetValue(value);
            }
        }

        private sealed class ActorCommand : CommandBase
        {
            private readonly AtomicBoolean _canExecute  = new();
            private readonly AtomicBoolean _deactivated = new();
            private readonly Dispatcher    _dispatcher;
            private readonly string        _name;
            private readonly IActorRef     _self;

            public ActorCommand(string name, IActorRef self, CommandQuery? canExecute, Dispatcher dispatcher)
            {
                _name       = name;
                _self       = self;
                _dispatcher = dispatcher;
                if (canExecute == null)
                    _canExecute.GetAndSet(true);
                else
                {
                    canExecute.Monitor(b =>
                                       {
                                           if (_deactivated.Value)
                                               return;
                                           _canExecute.GetAndSet(b);

                                           _dispatcher.Invoke(RaiseCanExecuteChanged);
                                       });

                    _canExecute.GetAndSet(canExecute.Run());
                }
            }

            public override void Execute(object? parameter) => _self.Tell(new CommandExecuteEvent(_name, MayNotNull(parameter)));

            public override bool CanExecute(object? parameter) => _canExecute;

            public void Deactivate()
            {
                _deactivated.GetAndSet(true);
                _canExecute.GetAndSet(false);
                _dispatcher.Invoke(RaiseCanExecuteChanged);
            }
        }

        private sealed record ReviveActor(KeyValuePair<string, PropertyData>[] Data);

        #region Dispatcher

        public Dispatcher Dispatcher { get; }

        protected void UICall(Action<IUntypedActorContext> executor)
        {
            var context = Context;
            Dispatcher.Invoke(() => executor(context));
        }

        protected void UICall(Action executor) 
            => Dispatcher.Invoke(executor);

        protected Task<T> UICall<T>(Func<Task<T>> executor) => Dispatcher.Invoke(executor);

        protected Task<T> UICall<T>(Func<IUntypedActorContext, Task<T>> executor)
        {
            var context = Context;
            return Dispatcher.Invoke(() => executor(context));
        }

        #endregion

        #region Commands

        private void CommandExecute(CommandExecuteEvent obj)
        {
            var (name, parameter) = obj;

            var mayRegistration = _actorData.ObjectState.CommandRegistrations.Lookup(name);

            Match(mayRegistration,
                registration =>
                {
                    Log.Info("Execute Command {Commanf}", name);
                    registration.Command(parameter);
                },
                () => Log.Error("Command not Found {Name}", name));

            Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void CanCommandExecute(CanCommandExecuteRequest obj)
        {
            var (name, _) = obj;

            var mayRegistration = _actorData.ObjectState.CommandRegistrations.Lookup(name);

            Match(mayRegistration,
                registration =>
                {
                    var invoker = registration.CanExecute;

                    Tell(Context.Sender,
                        Match(invoker,
                            q => new CanCommandExecuteRespond(name, q.Run),
                            () => May(new CanCommandExecuteRespond(name, () => true))));
                },
                () =>
                {
                    Log.Warning("No Command Found {Name}", name);
                    Tell(Context.Sender, new CanCommandExecuteRespond(name, () => false));
                });

        }
        
        protected void InvokeCommand(string name)
        {
            Do(from registration in _actorData.ObjectState.CommandRegistrations.Lookup(name)
                from query in Either(registration.CanExecute, TrueQuery.Instance)
                where query.Run()
                select Action(() => registration.Command(Maybe<object>.Nothing)));
        }

        protected CommandRegistrationBuilder NewCommad
            => new((key, command, canExecute) =>
                   {
                       var data = new PropertyData(new UIProperty<ICommand>(key)
                       {
                           InternalValue = May<object?>(new ActorCommand(key, Context.Self, canExecute, Dispatcher))
                       }.LockSet());

                       _actorData.Run(s =>
                           from state in s
                           select state with
                               {
                               Propertys = state.Propertys.Add(key, data),
                               CommandRegistrations = state.CommandRegistrations.Add(key, new CommandRegistration(command, MayNotNull(canExecute)))
                               });

                       PropertyValueChanged(data);

                   }, this);

        public void InvalidateRequerySuggested()
            => Dispatcher.BeginInvoke(new Action(CommandManager.InvalidateRequerySuggested), DispatcherPriority.ApplicationIdle);

        #endregion

        #region Lifecycle

        private void RestartActor(ReviveActor actor)
        {
            foreach (var (name, data) in actor.Data)
            {
                foreach (var actorRef in data.Subscriptors)
                    TrackProperty(new TrackPropertyEvent(name), actorRef);
            }
        }

        protected override void PreRestart(Exception reason, object message)
        {
            foreach (var registration in _actorData.ObjectState.CommandRegistrations)
            {
                _actorData.ObjectState.Propertys[registration.Key]
                   .PropertyBase
                   .InternalValue
                   .AsInstanceOf<ActorCommand>()
                   .Deactivate();
            }

            Self.Tell(new ReviveActor(_actorData.ObjectState.Propertys.ToArray()));

            base.PreRestart(reason, message);
        }

        protected virtual void ActorTermination(Terminated obj)
        {
        }

        protected override void PostStop()
        {
            Log.Info("UiActor Terminated {ActorType}", GetType());

            _actorData.Run(s =>
                from state in s
                select state with
                    {
                    CommandRegistrations = ImmutableDictionary<string, CommandRegistration>.Empty,
                    EventRegistrations = ImmutableGroupDictionary<string, InvokeHelper>.Empty,
                    Propertys = ImmutableDictionary<string, PropertyData>.Empty
                    });

            base.PostStop();
        }

        private Action<IUiActor>? _terminationCallback;

        void IUiActor.RegisterTerminationCallback(Action<IUiActor> callback)
        {
            if (_terminationCallback == null)
                _terminationCallback = callback;
            else
                _terminationCallback += callback;
        }

        protected void ShowWindow<TWindow>()
            where TWindow : Window
            => Dispatcher.Invoke(() => LifetimeScope.Resolve<TWindow>().Show());

        protected virtual void Initialize(InitEvent evt)
        { }

        protected virtual Task InitializeAsync(InitEvent evt)
        {
            Initialize(evt);
            return Task.CompletedTask;
        }

        protected virtual void ControlUnload(UnloadEvent obj)
        { }

        protected virtual void InitParentViewModel(InitParentViewModel obj)
        {
            ViewModelSuperviser.Get(Context.System)
                               .Create(obj.Model);
        }

        #endregion

        #region Events

        private void ExecuteEvent(ExecuteEventExent obj)
        {
            var (eventData, name) = obj;

            var mayReg = _actorData.ObjectState.EventRegistrations.Lookup(name);

            Match(mayReg,
                reg =>
                {
                    Log.Info("Execute Event {Name}", name);
                    reg.ForEach(e => e.Execute(eventData));
                },
                () => Log.Warning("Event Not found {Name}", name));
        }

        protected EventRegistrationBuilder RegisterEvent(string name)
        {
            return new(name, (s, del) => _actorData.Run(st =>
                from state in st
                select state with{ EventRegistrations = state.EventRegistrations.Add(s, new InvokeHelper(del))}));
        }

        #endregion

        #region Propertys

        public FluentPropertyRegistration<TData> RegisterProperty<TData>(string name)
        {
            ThrowIsSeald();
            if (_actorData.ObjectState.Propertys.ContainsKey(name))
                throw new InvalidOperationException("Property is Regitrated");

            return new FluentPropertyRegistration<TData>(name, this).WithDefaultValue(default!);
        }

        private void GetPropertyValue(GetValueRequest obj)
        {
            var mayProp = _actorData.ObjectState.Propertys.Lookup(obj.Name);

            Tell(Context.Sender,
                Match(mayProp,
                    p => new GetValueResponse(obj.Name, p.PropertyBase.InternalValue),
                    () => new GetValueResponse(obj.Name, Maybe<object?>.Nothing)));
        }

        private void SetPropertyValue(SetValue obj)
        {
            var mayProp = _actorData.ObjectState.Propertys.Lookup(obj.Name);

            var (_, value) = obj;

            Do(from prop in mayProp 
                where !Equals(prop.PropertyBase.InternalValue, value)
               select Action(() => prop.SetValue(value)));
        }

        private void PropertyValueChanged(PropertyData propertyData)
        {
            foreach (var actorRef in propertyData.Subscriptors)
                Tell(actorRef, new PropertyChangedEvent(propertyData.PropertyBase.Name, propertyData.PropertyBase.InternalValue));

            propertyData.Error =
                from validator in propertyData.PropertyBase.Validator
                from result in validator(propertyData.PropertyBase.InternalValue)
                select result;

            foreach (var actorRef in propertyData.Subscriptors)
                Tell(actorRef, new ValidatingEvent(propertyData.Error, propertyData.PropertyBase.Name));

            propertyData.PropertyBase.IsValidSetter(propertyData.Error.IsNothing());
        }

        private void TrackProperty(TrackPropertyEvent obj, IActorRef sender)
        {
            Log.Info("Track Property {Name}", obj.Name);

            var mayProp = _actorData.ObjectState.Propertys.Lookup(obj.Name);

            Do(from prop in mayProp 
                select Action(() =>
                {
                    prop.Subscriptors.Add(sender);
                    Context.WatchWith(sender, new PropertyTermination(Context.Sender, obj.Name));

                    if (prop.PropertyBase.InternalValue.IsNothing()) return;

                    sender.Tell(new PropertyChangedEvent(obj.Name, prop.PropertyBase.InternalValue));
                    sender.Tell(new ValidatingEvent(prop.Error, obj.Name));
                }));
        }

        private void PropertyTerminationHandler(PropertyTermination obj)
        {
            var (actorRef, name) = obj;

            Do( from prop in _actorData.ObjectState.Propertys.Lookup(name)
                select Action(() => prop.Subscriptors.Remove(actorRef)));
        }

        void IUiActor.RegisterProperty(UIPropertyBase prop)
        {
            var data = new PropertyData(prop);
            data.PropertyBase.PriorityChanged += () => PropertyValueChanged(data);

            _actorData.Run(s =>
                from state in s 
                select state with{Propertys = state.Propertys.Add(prop.Name, data)});
        }

        #endregion
    }
}