using System.Collections.Generic;

namespace ServiceManager.ProjectRepository.Data
{
    public sealed class ToDeleteRevision
    {
        public ToDeleteRevision(string current) => BuckedId = current;

        public ToDeleteRevision()
        {
            
        }

        public string BuckedId { get; set; } = string.Empty;
    }
}