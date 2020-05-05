using System;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    public sealed class ActionSuspensionTracker : ISuspensionTracker
    {
        private readonly Action<ToggleSuspendedMessage> _handler;

        public ActionSuspensionTracker(Action<ToggleSuspendedMessage> handler) 
            => _handler = handler;

        public void Suspended(ToggleSuspendedMessage msg) 
            => _handler(msg);
    }
}