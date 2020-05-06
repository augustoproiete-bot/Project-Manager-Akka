using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MGIHelper.UI
{
    public sealed class SimpleCommand : ICommand
    {
        private readonly Action _exec;
        private readonly Func<bool> _canExec;

        public SimpleCommand(Action exec, Func<bool> canExec)
        {
            _exec = exec;
            _canExec = canExec;
        }

        public bool CanExecute(object parameter) 
            => _canExec?.Invoke() ?? true;

        public void Execute(object parameter) 
            => Task.Run(_exec);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}