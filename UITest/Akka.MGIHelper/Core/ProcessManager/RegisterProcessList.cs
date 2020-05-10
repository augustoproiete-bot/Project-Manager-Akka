using System.Collections.Immutable;
using Akka.Actor;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    [Record]
    public sealed partial class RegisterProcessList
    {
        public IActorRef Intrest { get; }

        public ImmutableArray<string> Files { get; }
    }
}