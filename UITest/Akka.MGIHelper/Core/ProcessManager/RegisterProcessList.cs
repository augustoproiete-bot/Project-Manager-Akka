using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class RegisterProcessList
    {
        public RegisterProcessList(IActorRef intrest, ImmutableArray<string> files)
        {
            Intrest = intrest;
            Files = files.Select(e => e.Trim()).ToImmutableArray();
        }

        public IActorRef Intrest { get; }

        public ImmutableArray<string> Files { get; }
    }
}