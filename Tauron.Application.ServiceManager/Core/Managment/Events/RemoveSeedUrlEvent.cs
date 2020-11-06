using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class RemoveSeedUrlEvent : MutatingChange
    {
        public string SeedUrl { get; }

        public int RemaningCount { get; }

        public RemoveSeedUrlEvent(string seedUrl, int remaningCount)
        {
            SeedUrl = seedUrl;
            RemaningCount = remaningCount;
        }
    }
}