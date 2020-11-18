using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Util;
using Akka.Util.Internal;
using Autofac;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public abstract class UiActor : ExposedReceiveActor
    {
        private readonly Dictionary<string, CommandRegistration> _commandRegistrations = new();
        private readonly GroupDictionary<string, InvokeHelper>   _eventRegistrations   = new();
        private readonly Dictionary<string, PropertyData>        _propertys            = new();
        private          bool                                    _isSeald;

        protected UiActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher)
        {
            LifetimeScope = lifetimeScope;
            Dispatcher    = dispatcher;
            InitHandler();
        }

        //internal IUntypedActorContext UIActorContext => Context;

        public ILifetimeScope LifetimeScope { get; }

        public override void AroundPreStart()
        {
            _isSeald = true;
            base.AroundPreStart();
        }

        internal void ThrowIsSeald()
        {
            if (_isSeald)
                throw new InvalidOperationException("The Ui Actor is immutale");
        }

        protected void InitHandler()
        {
            Receive<CanCommandExecuteRequest>(CanCommandExecute);
            Receive<CommandExecuteEvent>(CommandExecute);
            Receive<ControlSetEvent>(msg => SetControl(msg.Name, msg.Element));
            Receive<MakeEventHook>(msg => Context.Sender.Tell(Context.GetOrCreateEventActor(msg.Name + "-EventActor")));
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

        private sealed class PropertyTermination
        {
            public PropertyTermination(IActorRef actorRef, string name)
            {
                ActorRef = actorRef;
                Name     = name;
            }

            public IActorRef ActorRef { get; }

            public string Name { get; }
        }

        private sealed class CommandRegistration
        {
            public CommandRegistration(Action<object?> command, CommandQuery? canExecute)
            {
                Command    = command;
                CanExecute = canExecute;
            }

            public Action<object?> Command { get; }

            public CommandQuery? CanExecute { get; }
        }

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

            public string? Error { get; set; }

            public List<IActorRef> Subscriptors { get; } = new();

            public void SetValue(object value)
            {
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

            public override void Execute(object parameter) => _self.Tell(new CommandExecuteEvent(_name, parameter));

            public override bool CanExecute(object parameter) => _canExecute;

            public void Deactivate()
            {
                _deactivated.GetAndSet(true);
                _canExecute.GetAndSet(false);
                _dispatcher.Invoke(RaiseCanExecuteChanged);
            }
        }

        private sealed class ReviveActor
        {
            public ReviveActor(KeyValuePair<string, PropertyData>[] data) => Data = data;
            public KeyValuePair<string, PropertyData>[] Data { get; }
        }

        #region Dispatcher

        public Dispatcher Dispatcher { get; }

        protected void UICall(Action<IUntypedActorContext> executor)
        {
            var context = Context;
            Dispatcher.Invoke(() => executor(context));
        }

        protected void UICall(Action executor)
        {
            Dispatcher.Invoke(executor);
        }

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
            if (_commandRegistrations.TryGetValue(name, out var registration))
            {
                Log.Info("Execute Command {Commanf}", name);
                registration.Command(parameter);
            }
            else
                Log.Error("Command not Found {Name}", name);

            Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void CanCommandExecute(CanCommandExecuteRequest obj)
        {
            var (name, _) = obj;
            if (!_commandRegistrations.TryGetValue(name, out var registration))
            {
                Log.Warning("No Command Found {Name}", name);
                Context.Sender.Tell(new CanCommandExecuteRespond(name, () => false));
            }
            else
            {
                var invoker = registration.CanExecute;
                Context.Sender.Tell(invoker == null ? new CanCommandExecuteRespond(name, () => true) : new CanCommandExecuteRespond(name, () => invoker.Run()));
            }
        }

        protected void InvokeCommand(string name)
        {
            if (!_commandRegistrations.TryGetValue(name, out var cr))
                return;

            if (cr.CanExecute?.Run() ?? true)
                cr.Command(null);
        }

        protected CommandRegistrationBuilder NewCommad
            => new((key, command, canExecute) =>
                   {
                       var data = new PropertyData(new UIProperty<ICommand>(key)
                                                   {
                                                       InternalValue = new ActorCommand(key, Context.Self, canExecute, Dispatcher)
                                                   }.LockSet());

                       _propertys.Add(key, data);

                       PropertyValueChanged(data);

                       _commandRegistrations.Add(key, new CommandRegistration(command, canExecute));
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
            foreach (var registration in _commandRegistrations)
            {
                _propertys[registration.Key]
                   .PropertyBase
                   .InternalValue
                   .AsInstanceOf<ActorCommand>()
                   .Deactivate();
            }

            Self.Tell(new ReviveActor(_propertys.ToArray()));

            base.PreRestart(reason, message);
        }

        protected virtual void ActorTermination(Terminated obj)
        {
        }

        protected override void PostStop()
        {
            Log.Info("UiActor Terminated {ActorType}", GetType());
            _commandRegistrations.Clear();
            _eventRegistrations.Clear();
            _propertys.Clear();

            base.PostStop();
        }

        private Action<UiActor>? _terminationCallback;

        internal void RegisterTerminationCallback(Action<UiActor> callback)
        {
            if (_terminationCallback == null)
                _terminationCallback = callback;
            else
                _terminationCallback += callback;
        }

        protected void ShowWindow<TWindow>()
            where TWindow : Window
        {
            Dispatcher.Invoke(() => LifetimeScope.Resolve<TWindow>().Show());
        }

        protected virtual void Initialize(InitEvent evt)
        {
        }

        protected virtual Task InitializeAsync(InitEvent evt)
        {
            Initialize(evt);
            return Task.CompletedTask;
        }

        protected virtual void ControlUnload(UnloadEvent obj)
        {
        }

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
            if (_eventRegistrations.TryGetValue(name, out var reg))
            {
                Log.Info("Execute Event {Name}", name);
                reg.ForEach(e => e.Execute(eventData));
            }
            else
                Log.Warning("Event Not found {Name}", name);
        }

        protected EventRegistrationBuilder RegisterEvent(string name)
        {
            return new(name, (s, del) => _eventRegistrations.Add(s, new InvokeHelper(del)));
        }

        #endregion

        #region Propertys

        protected internal FluentPropertyRegistration<TData> RegisterProperty<TData>(string name)
        {
            ThrowIsSeald();
            if (_propertys.ContainsKey(name))
                throw new InvalidOperationException("Property is Regitrated");

            return new FluentPropertyRegistration<TData>(name, this).WithDefaultValue(default!);
        }

        private void GetPropertyValue(GetValueRequest obj)
        {
            Context.Sender.Tell(_propertys.TryGetValue(obj.Name, out var propertyData)
                                    ? new GetValueResponse(obj.Name, propertyData.PropertyBase.InternalValue)
                                    : new GetValueResponse(obj.Name, null));
        }

        private void SetPropertyValue(SetValue obj)
        {
            if (!_propertys.TryGetValue(obj.Name, out var propertyData))
                return;

            var (_, value) = obj;

            if (Equals(propertyData.PropertyBase.InternalValue, value)) return;

            propertyData.SetValue(value!);
        }

        private void PropertyValueChanged(PropertyData propertyData)
        {
            foreach (var actorRef in propertyData.Subscriptors)
                actorRef.Tell(new PropertyChangedEvent(propertyData.PropertyBase.Name, propertyData.PropertyBase.InternalValue));

            propertyData.Error = propertyData.PropertyBase.Validator?.Invoke(propertyData.PropertyBase.InternalValue);

            foreach (var actorRef in propertyData.Subscriptors)
                actorRef.Tell(new ValidatingEvent(propertyData.Error, propertyData.PropertyBase.Name));

            propertyData.PropertyBase.IsValidSetter(string.IsNullOrWhiteSpace(propertyData.Error));
        }

        private void TrackProperty(TrackPropertyEvent obj, IActorRef sender)
        {
            Log.Info("Track Property {Name}", obj.Name);

            if (!_propertys.TryGetValue(obj.Name, out var prop)) return;

            prop.Subscriptors.Add(sender);
            Context.WatchWith(sender, new PropertyTermination(Context.Sender, obj.Name));

            if (prop.PropertyBase.InternalValue == null) return;

            sender.Tell(new PropertyChangedEvent(obj.Name, prop.PropertyBase.InternalValue));
            sender.Tell(new ValidatingEvent(prop.Error, obj.Name));
        }

        private void PropertyTerminationHandler(PropertyTermination obj)
        {
            if (!_propertys.TryGetValue(obj.Name, out var prop)) return;
            prop.Subscriptors.Remove(obj.ActorRef);
        }

        internal void RegisterProperty(UIPropertyBase prop)
        {
            var data = new PropertyData(prop);
            data.PropertyBase.PriorityChanged += () => PropertyValueChanged(data);

            _propertys.Add(prop.Name, data);
        }

        #endregion
    }
}