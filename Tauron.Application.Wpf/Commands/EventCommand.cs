using System;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Commands
{
    [PublicAPI]
    public class EventCommand : CommandBase
    {
        public event Func<object?, bool>? CanExecuteEvent;

        public event Action<object?>? ExecuteEvent;

        public sealed override bool CanExecute(object parameter)
        {
            return OnCanExecute(parameter);
        }

        public sealed override void Execute(object parameter)
        {
            OnExecute(parameter);
        }

        protected virtual bool OnCanExecute(object? parameter)
        {
            var handler = CanExecuteEvent;
            return handler == null || handler(parameter);
        }

        protected virtual void OnExecute(object? parameter)
        {
            ExecuteEvent?.Invoke(parameter);
        }
    }
}