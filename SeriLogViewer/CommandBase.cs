using System;
using System.Windows.Input;
using JetBrains.Annotations;

namespace SeriLogViewer
{
    public abstract class CommandBase : ICommand
    {
        public virtual event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public virtual bool CanExecute([CanBeNull] object parameter)
        {
            return true;
        }

        public abstract void Execute([CanBeNull] object parameter);

        [UsedImplicitly]
        public virtual void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}