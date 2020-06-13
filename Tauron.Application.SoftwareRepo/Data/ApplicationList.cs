using System.Collections.Immutable;
using System.Linq;
using Amadevus.RecordGenerator;

namespace Tauron.Application.SoftwareRepo.Data
{
    [Record]
    public sealed partial class ApplicationList
    {
        internal ApplicationList(ApplicationList backup)
        {
            ApplicationEntries = ImmutableList<ApplicationEntry>.Empty.AddRange(backup.ApplicationEntries.Select(e => new ApplicationEntry(e)));
            Description = backup.Description;
            Name = backup.Name;
        }

        public ApplicationList()
        {
            Name = string.Empty;
            Description = string.Empty;
            ApplicationEntries = ImmutableList<ApplicationEntry>.Empty;
        }

        public string Name { get; }

        public string Description { get; }

        public ImmutableList<ApplicationEntry> ApplicationEntries { get; }
    }
}