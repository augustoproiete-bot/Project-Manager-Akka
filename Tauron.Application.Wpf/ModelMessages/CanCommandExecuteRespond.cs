using System;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class CanCommandExecuteRespond
    {
        public string Name { get; }

        public Func<bool> CanExecute { get; }

        public CanCommandExecuteRespond(string name, Func<bool> canExecute)
        {
            Name = name;
            CanExecute = canExecute;
        }
    }
}