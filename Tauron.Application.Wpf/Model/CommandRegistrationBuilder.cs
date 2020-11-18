using System;
using System.Collections.Generic;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class CommandRegistrationBuilder
    {
        private readonly Action<string, Action<Maybe<object>>, CommandQuery> _register;

        private List<CommandQuery> _canExecute = new();

        private Delegate? _command;

        internal CommandRegistrationBuilder(Action<string, Action<Maybe<object>>, CommandQuery> register, IExposedReceiveActor target)
        {
            Target    = target;
            _register = register;
        }

        public IExposedReceiveActor Target { get; }

        public CommandRegistrationBuilder WithExecute(Action<Maybe<object>> execute, Func<CommandQueryBuilder, IEnumerable<CommandQuery>> canExecute)
        {
            _command = _command.Combine(execute);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action execute, Func<CommandQueryBuilder, IEnumerable<CommandQuery>> canExecute)
        {
            _command = _command.Combine(new ActionMapper(execute).Action);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action<Maybe<object>> execute, Func<CommandQueryBuilder, CommandQuery> canExecute)
        {
            _command = _command.Combine(execute);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action<Maybe<object>> execute)
        {
            _command = _command.Combine(execute);

            return this;
        }

        public CommandRegistrationBuilder WithExecute(Action execute, Func<CommandQueryBuilder, CommandQuery> canExecute)
        {
            _command = _command.Combine(new ActionMapper(execute).Action);

            return WithCanExecute(canExecute);
        }

        public CommandRegistrationBuilder WithExecute(Action execute)
        {
            _command = _command.Combine(new ActionMapper(execute).Action);
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

            _register(name, (Action<Maybe<object>>) _command, CommandQueryBuilder.Instance.Combine(_canExecute.ToArray()));
        }

        private sealed class ActionMapper
        {
            private readonly Action _action;

            public ActionMapper(Action action) => _action = action;

            public Action<Maybe<object>> Action => ActionImpl;

            private void ActionImpl(Maybe<object> o) => _action();
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