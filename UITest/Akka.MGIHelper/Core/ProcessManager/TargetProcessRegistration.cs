using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Akka.Actor;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class TargetProcessRegistration
    {
        public ImmutableArray<string> FileNames { get; }

        public IActorRef Target { get; }

        //void Found(Process p);

        //void Exit(Process p);
    }
}