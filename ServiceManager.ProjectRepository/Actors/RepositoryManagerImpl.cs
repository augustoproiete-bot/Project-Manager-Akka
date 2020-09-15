using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : RepositoryManager
    {
        private readonly IMongoCollection<RepositoryEntry> _repositoryData;

        private readonly GridFSBucket _gridFsBucket;

        public RepositoryManagerImpl(IMongoClient client)
        {
            var database = client.GetDatabase("Repository");

            Receive<RegisterRepository>(RegisterRepository);

            _repositoryData = database.GetCollection<RepositoryEntry>("Repositorys");
            _gridFsBucket = new GridFSBucket(database);
        }

        private void RegisterRepository(RegisterRepository registerRepository)
        {

        }
    }
}
