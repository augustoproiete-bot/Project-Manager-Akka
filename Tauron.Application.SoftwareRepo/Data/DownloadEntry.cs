using System;
using Amadevus.RecordGenerator;

namespace Tauron.Application.SoftwareRepo.Data
{
    [Record]
    public sealed partial class DownloadEntry
    {
        internal DownloadEntry(DownloadEntry entry)
        {
            Version = entry.Version;
            Url = entry.Url;
        }

        public Version Version { get; }

        public string Url { get; }
    }
}