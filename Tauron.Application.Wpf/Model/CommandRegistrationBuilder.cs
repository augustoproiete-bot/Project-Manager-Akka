using System;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class CommandRegistrationBuilder
    {
        public ExposedReceiveActor Target { get; }
        private readonly Action<string, Action<object?>, Func<object?, bool>?> _register;

        private sealed class ActionMapper
        {
            private readonly Action _action;

            public ActionMapper(Action action) => _action = action;

            private void ActionImpl(object? o) => _action();

            public Action<object?> Action => ActionImpl;
        }

        private sealed class FuncMapper
        {
            private readonly Func<bool> _action;

            public FuncMapper(Func<bool> action) => _action = action;

            private bool ActionImpl(object? o) => _action();

            public Func<object?, bool> Action => ActionImpl;
        }

        private Delegate? _command;

        private Delegate? _canExecute;

        internal CommandRegistrationBuilder(Action<string, Action<object?>, Func<object?, bool>?> register, ExposedReceiveActor target)
        {
            Target = target;
            _register = register;
        }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute, Func<object?, bool>? canExecute)
        {
            _command = Delegate.Combine(_command, execute);
            _canExecute = Delegate.Combine(_canExecute, canExecute);

            return this;
        }

        public CommandRegistrationBuilder WithExecute(Action execute, Func<bool> canExecute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);
            _canExecute = Delegate.Combine(_canExecute, new FuncMapper(canExecute).Action);

            return this;
        }

        public CommandRegistrationBuilder WithCanExecute(Func<object?, bool> execute)
        {
            _canExecute = Delegate.Combine(_canExecute, execute);

            return this;
        }

        public CommandRegistrationBuilder WithCanExecute(Func<bool> execute)
        {
            _canExecute = Delegate.Combine(_canExecute, new FuncMapper(execute).Action);

            return this;
        }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute)
        {
            _command = Delegate.Combine(_command, execute);

            return this;
        }

        public CommandRegistrationBuilder WithExecute(Action execute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);

            return this;
        }

        public void ThenRegister(string name)
        {
            if(_command == null) return;

            _register(name, (Action<object?>) _command, _canExecute as Func<object?, bool>);
        }
    }
}