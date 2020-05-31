using System;
using System.ComponentModel;
using System.Diagnostics;
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

        public static readonly DependencyProperty TargetCommandProperty =
            DependencyProperty.RegisterAttached("TargetCommand", typeof(ICommand), typeof(CommandBinder), new UIPropertyMetadata(null, OnCommandStadeChanged));

        public static readonly DependencyProperty UseDirectProperty = DependencyProperty.RegisterAttached(
            "UseDirect", typeof(bool), typeof(CommandBinder), new UIPropertyMetadata(false, OnCommandStadeChanged));

        //private static readonly List<RoutedCommand> Commands = new List<RoutedCommand>();

        private static bool _isIn;

        //public static bool AutoRegister { get; set; } = true;

        public static ICommand? Find(string name)
        {
            //var val = Commands.Find(com => com.Name == name);
            //if (val == null && AutoRegister) val = ThenRegister(name, name);

            //return val;

            return new EventCommand();
        }

        public static string GetCommand(DependencyObject obj) => (string) Argument.NotNull(obj, nameof(obj)).GetValue(CommandProperty);


        public static string GetCustomPropertyName(DependencyObject obj) => (string) Argument.NotNull(obj, nameof(obj)).GetValue(CustomPropertyNameProperty);

        public static ICommand? GetTargetCommand(DependencyObject obj) => (ICommand) Argument.NotNull(obj, nameof(obj)).GetValue(TargetCommandProperty);

        public static bool GetUseDirect(DependencyObject obj) => (bool) Argument.NotNull(obj, nameof(obj)).GetValue(UseDirectProperty);


        public static void SetCommand(DependencyObject obj, string value) => Argument.NotNull(obj, nameof(obj)).SetValue(CommandProperty, value);

        public static void SetCustomPropertyName(DependencyObject obj, string value) => Argument.NotNull(obj, nameof(obj)).SetValue(CustomPropertyNameProperty, value);

        public static void SetTargetCommand(DependencyObject obj, ICommand value) => Argument.NotNull(obj, nameof(obj)).SetValue(TargetCommandProperty, value);

        public static void SetUseDirect(DependencyObject obj, bool value) => Argument.NotNull(obj, nameof(obj)).SetValue(UseDirectProperty, value);

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

        private static readonly Random Rnd = new Random();

        private static void BindInternal(string? oldValue, string? newValue, IBinderControllable binder, DependencyObject affectedPart)
        {
            _isIn = true;

            if (oldValue != null)
                binder.CleanUp(NamePrefix + oldValue);
            if (newValue == null) return;

            var name = NamePrefix + newValue + "#" + Rnd.Next();
            if (newValue != null && newValue.Contains(':'))
            {
                var vals = newValue.Split(new[] {':'}, 2);
                newValue = vals[1];

                var command = Find(vals[0]);
                if (command != null) SetTargetCommand(affectedPart, command);
            }
            else if (GetTargetCommand(affectedPart) == null && newValue != null)
            {
                var command = Find(newValue);
                if (command != null) SetTargetCommand(affectedPart, command);
            }
            else
            {
                var command = GetTargetCommand(affectedPart);
                if (command is RoutedCommand routedCommand)
                    name = NamePrefix + routedCommand.Name + "#" + Rnd.Next();
                else
                    name = NamePrefix + command + "#" + Rnd.Next();
            }

            var newlinker = new CommandLinker {CommandTarget = newValue};
            binder.Register(name, newlinker, affectedPart);
            _isIn = false;
        }

        private class CommandLinker : ControlBindableBase
        {
            private CommandFactory? _factory;
            private PropertySearcher? _searcher;

            public string? CommandTarget { get; set; }

            protected override void CleanUp()
            {
                _factory?.Free();
            }

            protected override void Bind(object dataContext)
            {
                var commandTarget = CommandTarget;
                if (commandTarget == null)
                {
                    Log.Logger.Error("CommandBinder: No Binding: {CommandTarget}", commandTarget);
                    return;
                }

                var customProperty = GetCustomPropertyName(AffectedObject);
                var useDirect = GetUseDirect(AffectedObject);
                if (!(GetTargetCommand(AffectedObject) is EventCommand targetCommand))
                {
                    Log.Logger.Error("CommandBinder: No ICommand: {CommandTarget}", commandTarget);
                    return;
                }

                if (_factory == null)
                {
                    _factory = new CommandFactory(dataContext as IViewModel);
                }
                else
                {
                    _factory.DataContext = dataContext as IViewModel;
                }

                if (!useDirect)
                    _factory.Connect(targetCommand, commandTarget);

                if (_searcher == null)
                {
                    _searcher = new PropertySearcher(AffectedObject, customProperty, targetCommand);
                }
                else
                {
                    _searcher.CustomName = customProperty;
                    _searcher.Command = _factory.LastCommand;
                }

                _searcher.SetCommand();
            }

            private class CommandFactory
            {
                private Func<object?, bool>? _canExecute;

                private Action<object?>? _execute;

                private Func<bool>? _responded;

                public CommandFactory(IViewModel? dataContext) 
                    => DataContext = dataContext;

                public EventCommand? LastCommand { get; private set; }

                public IViewModel? DataContext { private get; set; }

                public void Free()
                {
                    _responded = null;

                    if(LastCommand == null) return;

                    LastCommand.CanExecuteEvent += _canExecute;
                    LastCommand.ExecuteEvent += _execute;

                    LastCommand = null;
                }

                public void Connect(EventCommand command, string commandName)
                {
                    _responded = null;
                    LastCommand = command;

                    if (DataContext == null) return;

                    _execute = parm => DataContext.Tell(new CommandExecuteEvent(commandName, parm));
                    _canExecute = parm =>
                               {
                                   try
                                   {
                                       if (_responded == null)
                                       {
                                           if (!DataContext.IsInitialized) return false;

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

                                Debug.Print("CommandBinder: FoundetProperty Incompatible: {0}:{1}", typeName, propName);
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