using System;
using System.Windows.Input;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Commands
{
    [PublicAPI]
    public abstract class CommandBase : ICommand
    {
        public virtual event EventHandler? CanExecuteChanged;

        protected CommandBase() => CommandManager.RequerySuggested += (_, _) => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public virtual bool CanExecute([CanBeNull] object? parameter) => true;

        public abstract void Execute([CanBeNull] object? parameter);

        public virtual void RaiseCanExecuteChanged() 
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}