using System.Collections.Immutable;
using Akka.Actor;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class TargetProcessRegistration
    {
        public ImmutableArray<string> FileNames { get; } = ImmutableArray<string>.Empty;

        public IActorRef Target { get; } = ActorRefs.Nobody;

        //void Found(Process p);

        //void Exit(Process p);
    }
}