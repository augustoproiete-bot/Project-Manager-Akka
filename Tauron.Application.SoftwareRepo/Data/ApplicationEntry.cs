using System;
using System.Collections.Immutable;
using System.Linq;
using Amadevus.RecordGenerator;

namespace Tauron.Application.SoftwareRepo.Data
{
    [Record]
    public sealed partial class ApplicationEntry
    {
        public ApplicationEntry(ApplicationEntry entry)
        {
            BranchName = entry.BranchName;
            RepositoryName = entry.RepositoryName;
            Name = entry.Name;
            Last = entry.Last;
            Id = entry.Id;

            Downloads = ImmutableList<DownloadEntry>.Empty.AddRange(entry.Downloads.Select(e => new DownloadEntry(e)));
        }

        public string Name { get; }

        public Version Last { get; }

        public long Id { get; }

        public ImmutableList<DownloadEntry> Downloads { get; }

        public string RepositoryName { get; }

        public string BranchName { get; }
    }
}