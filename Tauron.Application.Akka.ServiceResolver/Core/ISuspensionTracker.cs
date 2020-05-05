using System;
using Tauron.Application.Akka.ServiceResolver.Messages.Global;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    public interface ISuspensionTracker
    {
        void Suspended(ToggleSuspendedMessage msg);
    }
}