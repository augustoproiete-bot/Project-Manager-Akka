using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class CommandBinder
    {
        private const string NamePrefix = "CommandBinder.";

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(string), typeof(CommandBinder), new UIPropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty CustomPropertyNameProperty =
            DependencyProperty.RegisterAttached("CustomPropertyName", typeof(string), typeof(CommandBinder), new UIPropertyMetadata("Command", OnCommandStadeChanged));

        private static ILogger _baseLogger = Log.Logger.ForContext(typeof(CommandBinder));

        private static bool _isIn;

        private static readonly Random Rnd = new Random();

        public static string GetCommand(DependencyObject obj) => (string) Argument.NotNull(obj, nameof(obj)).GetValue(CommandProperty);


        public static string GetCustomPropertyName(DependencyObject obj) => (string) Argument.NotNull(obj, nameof(obj)).GetValue(CustomPropertyNameProperty);
        
        public static void SetCommand(DependencyObject obj, string value) => Argument.NotNull(obj, nameof(obj)).SetValue(CommandProperty, value);

        public static void SetCustomPropertyName(DependencyObject obj, string value) => Argument.NotNull(obj, nameof(obj)).SetValue(CustomPropertyNameProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;

            var root = ControlBindLogic.FindRoot(d);
            if (root == null)
            {
                if (!(d is FrameworkElement element)) return;

                ControlBindLogic.MakeLazy(element, e.NewValue as string, e.OldValue as string, BindInternal);
                return;
            }

            BindInternal(e.OldValue as string, e.NewValue as string, root, d);
        }

        private static void OnCommandStadeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isIn) return;

            var root = ControlBindLogic.FindRoot(d);
            if (root == null) return;

            var name = GetCommand(d);
            BindInternal(name, name, root, d);
        }

        private static void BindInternal(string? oldValue, string? newValue, IBinderControllable binder, DependencyObject affectedPart)
        {
            _isIn = true;

            if (oldValue != null)
                binder.CleanUp(NamePrefix + oldValue + affectedPart.GetHashCode());
            if (newValue == null) return;

            var name = NamePrefix + newValue + affectedPart.GetHashCode();
            var newlinker = new CommandLinker(name) {CommandTarget = newValue};
            binder.Register(name, newlinker, affectedPart);
            _isIn = false;
        }

        private sealed class CommandLinker : ControlBindableBase
        {
            private readonly string _key;
            private static readonly ILogger Logger = Log.ForContext<CommandLinker>();

            private CommandFactory? _factory;
            private PropertySearcher? _searcher;

            public string? CommandTarget { get; set; }

            public CommandLinker(string key)
            {
                _key = key;
            }

            protected override void CleanUp()
            {
                Logger.Debug("Clean Up Command {Name}", _key);
                _factory?.Free();
            }

            protected override void Bind(object dataContext)
            {
                Logger.Debug("Bind Command {Name}", _key);
                var commandTarget = CommandTarget;
                if (commandTarget == null)
                {
                    Log.Logger.Error("CommandBinder: No Binding: {CommandTarget} -- {Name}", commandTarget, _key);
                    return;
                }

                var customProperty = GetCustomPropertyName(AffectedObject);
                var targetCommand = new EventCommand();

                if (_factory == null)
                {
                    Logger.Debug("Create Command Factory {Name}", _key);
                    _factory = new CommandFactory(dataContext as IViewModel, Logger, _key);
                }
                else
                {
                    Logger.Debug("Update Command Factory {Name}", _key);
                    _factory.DataContext = dataContext as IViewModel;
                }

                _factory.Connect(targetCommand, commandTarget);

                if (_searcher == null)
                {
                    Logger.Debug("Create Property Searcher {Name}", _key);
                    _searcher = new PropertySearcher(AffectedObject, customProperty, targetCommand);
                }
                else
                {
                    Logger.Debug("Update Peroperty Sercher {Name}", _key);
                    _searcher.CustomName = customProperty;
                    _searcher.Command = _factory.LastCommand;
                }

                _searcher.SetCommand();
            }

            private class CommandFactory
            {
                private readonly ILogger _logger;
                private readonly string _key;
                private Func<object?, bool>? _canExecute;
                private Action<object?>? _execute;
                private Func<bool>? _responded;

                public CommandFactory(IViewModel? dataContext, ILogger logger, string key)
                {
                    _logger = logger;
                    _key = key;
                    DataContext = dataContext;
                }

                public EventCommand? LastCommand { get; private set; }

                public IViewModel? DataContext { private get; set; }

                public void Free()
                {
                    _logger.Debug("Remove Event Handler {Name}", _key);
                    _responded = null;

                    if (LastCommand == null) return;

                    LastCommand.Clear();
                    LastCommand = null;
                }

                public void Connect(EventCommand command, string commandName)
                {
                    _logger.Debug("Add Command Handler {Name}", _key);
                    _responded = null;
                    LastCommand = command;

                    if (DataContext == null) return;

                    _execute = parm =>
                               {
                                   _logger.Debug("Tell Execute Command {Name}", _key);
                                   DataContext.Tell(new CommandExecuteEvent(commandName, parm));
                               };
                    _canExecute = parm =>
                    {
                        try
                        {
                            if (_responded == null)
                            {
                                if (!DataContext.IsInitialized) return false;

                                _logger.Debug("Ask For Can Execute {Name}", _key);
                                _responded = DataContext
                                    .Ask<CanCommandExecuteRespond>(new CanCommandExecuteRequest(commandName, parm), TimeSpan.FromSeconds(1)).Result.CanExecute;
                            }

                            return _responded?.Invoke() ?? false;
                        }
                        catch (AggregateException)
                        {
                            return false;
                        }
                        catch (TimeoutException)
                        {
                            return false;
                        }
                    };

                    _logger.Debug("Attach Command Handler {Name}", _key);
                    LastCommand.Clear();
                    LastCommand.CanExecuteEvent += _canExecute;
                    LastCommand.ExecuteEvent += _execute;
                }
            }

            private class PropertySearcher
            {
                private PropertyFlags _changedFlags;
                private WeakReference<ICommand>? _command;
                private string? _customName;
                private PropertyInfo? _prop;

                public PropertySearcher(DependencyObject affectedObject, string customName, ICommand command)
                {
                    AffectedObject = Argument.NotNull(affectedObject, nameof(affectedObject));
                    CustomName = Argument.NotNull(customName, nameof(customName));
                    Command = Argument.NotNull(command, nameof(command));

                    _changedFlags = PropertyFlags.All;
                }

                private DependencyObject AffectedObject { get; }

                public ICommand? Command
                {
                    private get { return _command?.TypedTarget(); }
                    set
                    {
                        if (_command != null && _command.TypedTarget() == value) return;
                        if (value == null)
                        {
                            _command = null;
                        }
                        else
                        {
                            _command = new WeakReference<ICommand>(value);
                            _changedFlags |= PropertyFlags.Command;
                        }
                    }
                }

                public string? CustomName
                {
                    private get { return _customName; }
                    set
                    {
                        if (_customName == value) return;

                        _customName = value;
                        _changedFlags |= PropertyFlags.CustomName;
                    }
                }

                public void SetCommand()
                {
                    try
                    {
                        var commandChanged = _changedFlags.HasFlag(PropertyFlags.Command);
                        var customNameChanged = _changedFlags.HasFlag(PropertyFlags.CustomName);

                        if (customNameChanged)
                        {
                            var tarType = AffectedObject.GetType();
                            _prop = tarType.GetProperty(CustomName ?? string.Empty);
                            if (_prop != null
                                && (!_prop.CanWrite || !typeof(ICommand).IsAssignableFrom(_prop.PropertyType)))
                            {
                                var typeName = tarType.ToString();
                                var propName = _prop == null ? CustomName + "(Not Found)" : _prop.Name;

                                Log.Logger.Error("CommandBinder: FoundetProperty Incompatible: {0}:{1}", typeName, propName);
                                _prop = null;
                            }
                            else
                            {
                                commandChanged = true;
                            }
                        }

                        if (commandChanged && _prop != null)
                            _prop.SetInvokeMember(AffectedObject, Command);
                    }
                    finally
                    {
                        _changedFlags = PropertyFlags.None;
                    }
                }

                [Flags]
                private enum PropertyFlags
                {
                    None = 0,
                    CustomName = 1,
                    Command = 2,
                    All = 3
                }
            }
        }
    }
}