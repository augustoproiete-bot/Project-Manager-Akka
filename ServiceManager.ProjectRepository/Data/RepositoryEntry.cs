using MongoDB.Bson;

namespace ServiceManager.ProjectRepository.Data
{
    public sealed class RepositoryEntry
    {
        public string RepoName { get; set; }

        public string FileName { get; set; }

        public string SourceUrl { get; set; }
    }
}