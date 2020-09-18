using System;

namespace ServiceManager.ProjectRepository.Data
{
    public sealed class CleanUpTime
    {
        public TimeSpan Interval { get; set; }

        public DateTime Last { get; set; }
    }
}