using System;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Wpf.Model
{
    [PublicAPI]
    public sealed class EventRegistrationBuilder
    {
        private readonly string _name;
        private readonly Action<string, Delegate> _register;

        internal EventRegistrationBuilder(string name, Action<string, Delegate> register)
        {
            _name = name;
            _register = register;
        }

        public EventRegistrationBuilder WithEventHandler(Action handler)
        {
            _register(_name, handler);
            return this;
        }

        public EventRegistrationBuilder WithEventHandler(string name, Action<EventData> handler)
        {
            _register(_name, handler);
            return this;
        }

        public EventRegistrationBuilder WithEventHandler(string name, Action<object, EventArgs> handler)
        {
            _register(_name, handler);
            return this;
        }

        public EventRegistrationBuilder WithEventHandler(string name, Action<EventArgs> handler)
        {
            _register(_name, handler);
            return this;
        }
    }
}