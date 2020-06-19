﻿namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class CommandExecuteEvent
    {
        public string Name { get; }

        public object? Parameter { get; }

        public CommandExecuteEvent(string name, object? parameter)
        {
            Name = name;
            Parameter = parameter;
        }

        public void Deconstruct(out string name, out object? parameter)
        {
            name = Name;
            parameter = Parameter;
        }
    }
}