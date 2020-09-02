using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Util.Internal;
using Autofac;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public abstract class UiActor : ExposedReceiveActor
    {
        private readonly Dictionary<string, CommandRegistration> _commandRegistrations = new Dictionary<string, CommandRegistration>();
        private readonly GroupDictionary<string, InvokeHelper> _eventRegistrations = new GroupDictionary<string, InvokeHelper>();
        private readonly Dictionary<string, PropertyData> _propertys = new Dictionary<string, PropertyData>();
        private bool _isSeald;

        protected UiActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher)
        {
            LifetimeScope = lifetimeScope;
            Dispatcher = dispatcher;
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
            Receive<TrackPropertyEvent>(TrckProperty);
            Receive<Terminated>(ActorTermination);
            Receive<PropertyTermination>(PropertyTerminationHandler);
            Receive<UnloadEvent>(ControlUnload);
            Receive<InitParentViewModel>(InitParentViewModel);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Log.Error(reason, message.ToString());
            Context.System.Terminate();
            base.PreRestart(reason, message);
        }

        #region ControlEvents

        protected virtual void SetControl(string name, FrameworkElement element)
        {
        }

        #endregion

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

        protected Task<T> UICall<T>(Func<Task<T>> executor)
        {
            return Dispatcher.Invoke(executor);
        }

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
            var (name, parameter) = obj;
            if (!_commandRegistrations.TryGetValue(name, out var registration))
            {
                Log.Warning("No Command Found {Name}", name);
                Context.Sender.Tell(new CanCommandExecuteRespond(name, () => false));
            }
            else
            {
                var invoker = registration.CanExecute;
                Context.Sender.Tell(invoker == null ? new CanCommandExecuteRespond(name, () => true) : new CanCommandExecuteRespond(name, () => invoker(parameter)));
            }
        }

        protected void InvokeCommand(string name)
        {
            if(!_commandRegistrations.TryGetValue(name, out var cr))
                return;

            if (cr.CanExecute?.Invoke(null) ?? true) 
                cr.Command(null);
        }

        protected CommandRegistrationBuilder NewCommad
            => new CommandRegistrationBuilder(
                (key, command, canExecute) =>
                {
                    _propertys.Add(key, new PropertyData(new UIProperty<ICommand>(key)
                    {
                        InternalValue = new ActorCommand(key, Context.Self, canExecute)
                    }.LockSet()));
                    _commandRegistrations.Add(key, new CommandRegistration(command, canExecute));
                }, this);

        public void CommandChanged()
        {
            Dispatcher.BeginInvoke(new Action(CommandManager.InvalidateRequerySuggested), DispatcherPriority.ApplicationIdle);
        }

        #endregion

        #region Lifecycle

        protected virtual void ActorTermination(Terminated obj)
        {
        }

        protected override void PostStop()
        {
            Log.Info("UiActor Terminated {ActorType}", GetType());
            _commandRegistrations.Clear();
            _eventRegistrations.Clear();
            _propertys.Clear();
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

        private void InitParentViewModel(InitParentViewModel obj)
        {
            obj.Model.Init(Context);
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
            return new EventRegistrationBuilder(name, (s, del) => _eventRegistrations.Add(s, new InvokeHelper(del)));
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

            CommandChanged();
        }

        private void TrckProperty(TrackPropertyEvent obj)
        {
            Log.Info("Track Property {Name}", obj.Name);

            if (!_propertys.TryGetValue(obj.Name, out var prop)) return;

            prop.Subscriptors.Add(Sender);
            Context.WatchWith(Sender, new PropertyTermination(Context.Sender, obj.Name));

            if (prop.PropertyBase.InternalValue == null) return;

            Sender.Tell(new PropertyChangedEvent(obj.Name, prop.PropertyBase.InternalValue));
            Sender.Tell(new ValidatingEvent(prop.Error, obj.Name));
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

        private sealed class PropertyTermination
        {
            public PropertyTermination(IActorRef actorRef, string name)
            {
                ActorRef = actorRef;
                Name = name;
            }

            public IActorRef ActorRef { get; }

            public string Name { get; }
        }

        private sealed class CommandRegistration
        {
            public CommandRegistration(Action<object?> command, Func<object?, bool>? canExecute)
            {
                Command = command;
                CanExecute = canExecute;
            }

            public Action<object?> Command { get; }

            public Func<object?, bool>? CanExecute { get; }
        }

        private sealed class InvokeHelper
        {
            private readonly Delegate _method;
            private readonly MethodType _methodType;

            public InvokeHelper(Delegate del)
            {
                _method = del;
                var method = del.Method;

                _methodType = (MethodType)method.GetParameters().Length;
                if (_methodType != MethodType.One) return;
                if (method.GetParameters()[0].ParameterType != typeof(EventData)) _methodType = MethodType.EventArgs;
            }

            public void Execute(EventData? parameter)
            {
                var args = _methodType switch
                {
                    MethodType.Zero => new object[0],
                    MethodType.One => new object[] { parameter! },
                    MethodType.Two => new[] { parameter?.Sender, parameter?.EventArgs },
                    MethodType.EventArgs => new[] { parameter?.EventArgs },
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
            public PropertyData(UIPropertyBase propertyBase)
            {
                PropertyBase = propertyBase;
            }

            public UIPropertyBase PropertyBase { get; }

            public string? Error { get; set; }

            public List<IActorRef> Subscriptors { get; } = new List<IActorRef>();

            public void SetValue(object value)
            {
                PropertyBase.SetValue(value);
            }
        }

        private sealed class ActorCommand : CommandBase
        {
            private readonly string _name;
            private readonly IActorRef _self;
            private readonly Func<object?, bool> _canExecute;

            public ActorCommand(string name, IActorRef self, Func<object?, bool>? canExecute)
            {
                _name = name;
                _self = self;
                _canExecute = canExecute ?? (p => true);
            }

            public override void Execute(object parameter) => _self.Tell(new CommandExecuteEvent(_name, parameter));

            public override bool CanExecute(object parameter) => _canExecute(parameter);
        }
    }
}