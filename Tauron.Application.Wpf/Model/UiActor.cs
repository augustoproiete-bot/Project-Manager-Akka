using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Akka.Actor;
using Akka.Event;
using Autofac;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.ModelMessages;
using Tauron.Host;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public abstract class UiActor : ReceiveActor
    {
        private sealed class PropertyTermination
        {
            public IActorRef ActorRef { get; }

            public string Name { get; }

            public PropertyTermination(IActorRef actorRef, string name)
            {
                ActorRef = actorRef;
                Name = name;
            }
        }

        private sealed class CommandRegistration
        {
            public Action<object?> Command { get; }

            public Func<object?, bool>? CanExecute { get; }

            public CommandRegistration(Action<object?> command, Func<object?, bool>? canExecute)
            {
                Command = command;
                CanExecute = canExecute;
            }
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

            public void Execute(EventData parameter)
            {
                var args = _methodType switch
                {
                    MethodType.Zero => new object[0],
                    MethodType.One => new object[] { parameter },
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
            public object? Value { get; set; }

            public Func<object?, string?>? Validator { get; set; }

            public string? Error { get; set; }

            public List<IActorRef> Subscriptors { get; } = new List<IActorRef>();
        }

        private readonly Dictionary<string, CommandRegistration> _commandRegistrations = new Dictionary<string, CommandRegistration>();
        private readonly Dictionary<string, InvokeHelper> _eventRegistrations = new Dictionary<string, InvokeHelper>();
        private readonly Dictionary<string, PropertyData> _propertys = new Dictionary<string, PropertyData>();

        protected ILoggingAdapter Log { get; } = Context.GetLogger();
        protected ILifetimeScope LifetimeScope { get; }
        protected Dispatcher Dispatcher { get; }

        protected UiActor(ILifetimeScope lifetimeScope, Dispatcher dispatcher)
        {
            LifetimeScope = lifetimeScope;
            Dispatcher = dispatcher;
            InitHandler();
        }

        protected void InitHandler()
        {
            Receive<CanCommandExecuteRequest>(CanCommandExecute);
            Receive<CommandExecuteEvent>(CommandExecute);
            Receive<ControlSetEvent>(msg => SetControl(msg.Name, msg.Element));
            Receive<MakeEventHook>(msg => Context.Sender.Tell(Context.CreateEventActor()));
            Receive<ExecuteEventExent>(ExecuteEvent);
            Receive<GetValueRequest>(GetPropertyValue);
            ReceiveAsync<InitEvent>(async _ => await InitializeAsync());
            Receive<SetValue>(SetPropertyValue);
            Receive<TrackPropertyEvent>(TrckProperty);
            Receive<Terminated>(ActorTermination);
            Receive<PropertyTermination>(PropertyTerminationHandler);
            Receive<UnloadEvent>(ControlUnload);
            Receive<InitParentViewModel>(InitParentViewModel);
        }

        protected virtual void ActorTermination(Terminated obj)
        {

        }

        protected override void PostStop()
        {
            _commandRegistrations.Clear();
            _eventRegistrations.Clear();
            _propertys.Clear();
        }

        protected void ShowWindow<TWindow>()
            where TWindow : Window =>
            Dispatcher.Invoke(() =>  LifetimeScope.Resolve<TWindow>().Show());

        #region Commands

        private void CommandExecute(CommandExecuteEvent obj)
        {
            var (name, parameter) = obj;
            if (_commandRegistrations.TryGetValue(name, out var registration)) 
                registration.Command(parameter);
            else
                Log.Error("Command not Found {Name}", name);
        }

        private void CanCommandExecute(CanCommandExecuteRequest obj)
        {
            var (name, parameter) = obj;
            if (!_commandRegistrations.TryGetValue(name, out var registration))
            {
                Log.Warning("No Command Found {Name}", name);
                Context.Sender.Tell(new CanCommandExecuteRespond(name, false));
            }
            else
            {
                var invoker = registration.CanExecute;
                Context.Sender.Tell(invoker == null ? new CanCommandExecuteRespond(name, true) : new CanCommandExecuteRespond(name, invoker(parameter)));
            }
        }

        public void RegisterCommand(string name, Action<object?> execute, Func<object?, bool>? canExecute = null) 
            => _commandRegistrations[name] = new CommandRegistration(execute, canExecute);

        public void RegisterCommand(string name, Action execute, Func<bool> canExecute) 
            => _commandRegistrations[name] = new CommandRegistration(_ => execute(), _ => canExecute());

        #endregion

        #region Lifecycle

        protected virtual void Initialize() { }

        protected virtual Task InitializeAsync()
        {
            Initialize();
            return Task.CompletedTask;
        }

        protected virtual void ControlUnload(UnloadEvent obj) 
            => Context.Stop(Self);

        private void InitParentViewModel(InitParentViewModel obj) 
            => obj.Model.Init(Context);

        #endregion

        #region Events

        private void ExecuteEvent(ExecuteEventExent obj)
        {
            if(_eventRegistrations.TryGetValue(obj.Name, out var reg))
                reg.Execute(obj.Data);
        }

        protected void RegisterEventHandler(string name, Action handler)
            => _eventRegistrations[name] = new InvokeHelper(handler);

        protected void RegisterEventHandler(string name, Action<EventData> handler)
            => _eventRegistrations[name] = new InvokeHelper(handler);
        protected void RegisterEventHandler(string name, Action<object, EventArgs> handler)
            => _eventRegistrations[name] = new InvokeHelper(handler);
        protected void RegisterEventHandler(string name, Action<EventArgs> handler)
            => _eventRegistrations[name] = new InvokeHelper(handler);

        #endregion

        #region ControlEvents

        protected virtual void SetControl(string name, FrameworkElement element) { }

        #endregion

        #region Propertys

        private PropertyData GetOrAdd(string name)
        {
            if (_propertys.TryGetValue(name, out var propertyData)) return propertyData;
            
            propertyData = new PropertyData();
            _propertys[name] = propertyData;

            return propertyData;
        }

        private void GetPropertyValue(GetValueRequest obj) 
            => Context.Sender.Tell(_propertys.TryGetValue(obj.Name, out var propertyData) ? new GetValueResponse(obj.Name, propertyData.Value) : new GetValueResponse(obj.Name, null));

        private void SetPropertyValue(SetValue obj)
        {
            var (name, value) = obj;
            var propertyData = GetOrAdd(name);

            if (Equals(propertyData.Value, value)) return;

            propertyData.Value = value;
            foreach (var actorRef in propertyData.Subscriptors)
                actorRef.Tell(new PropertyChangedEvent(name, propertyData.Value));


            propertyData.Error = propertyData.Validator?.Invoke(propertyData.Value);

            foreach (var actorRef in propertyData.Subscriptors)
                actorRef.Tell(new ValidatingEvent(propertyData.Error, name));
        }

        private void TrckProperty(TrackPropertyEvent obj)
        {
            var prop = GetOrAdd(obj.Name);
            prop.Subscriptors.Add(Sender);
            Context.WatchWith(Sender, new PropertyTermination(Context.Sender, obj.Name));

            //TODO if(prop.Value == null) return;

            //Sender.Tell(new PropertyChangedEvent(obj.Name, prop.Value));
            //Sender.Tell(new ValidatingEvent(prop.Error, obj.Name));
        }

        private void PropertyTerminationHandler(PropertyTermination obj) 
            => GetOrAdd(obj.Name).Subscriptors.Remove(obj.ActorRef);

        [return: MaybeNull]
        protected TValue Get<TValue>([CallerMemberName] string? name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default!;

            if (_propertys.TryGetValue(name, out var propertyData) && propertyData.Value is TValue value)
                return value;
            return default!;
        }

        protected void Set<TValue>(TValue value, [CallerMemberName] string? name = null)
        {
            if(string.IsNullOrWhiteSpace(name)) return;
            SetPropertyValue(new SetValue(name, value));
        }

        protected void Set<TValue>(TValue value, Action onChange, [CallerMemberName] string? name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            SetPropertyValue(new SetValue(name, value));
            onChange();
        }

        protected void RegisterValidator<TValue>(string name, Func<TValue, string> validator)
        {
            var property = GetOrAdd(name);
            property.Validator = o =>
                                 {
                                     if (o is TValue value)
                                         return validator(value);
                                     return null;
                                 };
        }

        #endregion
    }
}