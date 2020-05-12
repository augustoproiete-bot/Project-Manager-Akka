using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed partial class RegisterProcessList
    {
        public IActorRef Intrest { get; }

        public ImmutableArray<string> Files { get; }


        public RegisterProcessList(IActorRef intrest, ImmutableArray<string> files)
        {
            Intrest = intrest;
            Files = files.Select(e => e.Trim()).ToImmutableArray();
        }
    }
}