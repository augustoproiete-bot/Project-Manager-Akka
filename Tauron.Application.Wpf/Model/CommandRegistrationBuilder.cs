using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class CommandRegistrationBuilder
    {
        private readonly Action<string, Action<object?>, CommandQuery?> _register;

        private List<CommandQuery> _canExecute = new List<CommandQuery>();

        private Delegate? _command;

        internal CommandRegistrationBuilder(Action<string, Action<object?>, CommandQuery?> register, IExposedReceiveActor target)
        {
            Target = target;
            _register = register;
        }

        public IExposedReceiveActor Target { get; }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute, Func<CommandQueryBuilder, IEnumerable<CommandQuery>> canExecute)
        {
            _command = Delegate.Combine(_command, execute);

            return canExecute != null ? WithCanExecute(canExecute) : this;
        }
        
        public CommandRegistrationBuilder WithExecute(Action execute, Func<CommandQueryBuilder, IEnumerable<CommandQuery>> canExecute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);

            return WithCanExecute(canExecute);
        }
        
        public CommandRegistrationBuilder WithExecute(Action<object?> execute, Func<CommandQueryBuilder, CommandQuery> canExecute)
        {
            _command = Delegate.Combine(_command, execute);

            return canExecute != null ? WithCanExecute(canExecute) : this;
        }

        public CommandRegistrationBuilder WithExecute(Action<object?> execute)
        {
            _command = Delegate.Combine(_command, execute);

            return this;
        }

        public CommandRegistrationBuilder WithExecute(Action execute, Func<CommandQueryBuilder, CommandQuery> canExecute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action execute)
        {
            _command = Delegate.Combine(_command, new ActionMapper(execute).Action);
            return this;
        }

        public CommandRegistrationBuilder WithCanExecute(Func<CommandQueryBuilder, IEnumerable<CommandQuery>> canExecute)
        {
            _canExecute.AddRange(canExecute(CommandQueryBuilder.Instance));

            return this;
        }

        public CommandRegistrationBuilder WithCanExecute(Func<CommandQueryBuilder, CommandQuery> canExecute)
        {
            _canExecute.Add(canExecute(CommandQueryBuilder.Instance));

            return this;
        }

        public void ThenRegister(string name)
        {
            if (_command == null) return;

            _register(name, (Action<object?>) _command, CommandQueryBuilder.Instance.Combine(_canExecute.ToArray()));
        }

        private sealed class ActionMapper
        {
            private readonly Action _action;

            public ActionMapper(Action action)
            {
                _action = action;
            }

            public Action<object?> Action => ActionImpl;

            private void ActionImpl(object? o)
            {
                _action();
            }
        }

        //private sealed class FuncMapper
        //{
        //    private readonly Func<bool> _action;

        //    public FuncMapper(Func<bool> action)
        //    {
        //        _action = action;
        //    }

        //    public Func<object?, bool> Action => ActionImpl;

        //    private bool ActionImpl(object? o)
        //    {
        //        return _action();
        //    }
        //}
    }
}