using System;
using System.Windows.Input;

namespace Akka.MGIHelper.Core
{
    public sealed class ActionCommand : ICommand
    {
        private readonly Action<object> _run;
        private readonly Func<object, bool>? _canRun;

        public ActionCommand(Action<object> run, Func<object, bool>? canRun = null)
        {
            _run = run;
            _canRun = canRun;
        }

        public ActionCommand(Action run, Func<bool>? canRun = null)
        {
            _run = _ => run();
            if(canRun != null)
                _canRun = _ => canRun();
        }

        public bool CanExecute(object parameter)
        {
            var result = _canRun?.Invoke(parameter);
            return result == null || result.Value;
        }

        public void Execute(object parameter) 
            => _run(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}