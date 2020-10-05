using MongoDB.Bson.Serialization.Attributes;

namespace ServiceManager.ProjectRepository.Data
{
    public sealed class RepositoryEntry
    {
        public string RepoName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string SourceUrl { get; set; } = string.Empty;

        [BsonId]
        public long RepoId { get; set; }

        public string LastUpdate { get; set; } = string.Empty;

        public bool IsUploaded { get; set; }
    }
}