using System.Collections.Immutable;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class TryJoinEvent : MutatingChange
    {
        public ImmutableList<string> Addreses { get; }

        public TryJoinEvent(ImmutableList<string> addreses) => Addreses = addreses;
    }
}