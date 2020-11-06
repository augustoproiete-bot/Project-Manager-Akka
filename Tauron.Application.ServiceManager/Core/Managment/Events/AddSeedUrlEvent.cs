using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class AddSeedUrlEvent : MutatingChange
    {
        public string SeedUrl { get; }

        public AddSeedUrlEvent(string seedUrl) => SeedUrl = seedUrl;
    }
}