using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : RepositoryManager
    {
        public RepositoryManagerImpl(IMongoClient client)
        {
            var database = client.GetDatabase("Repository");

            var repositoryData = database.GetCollection<RepositoryEntry>("Repositorys");
            var gridFsBucket = new GridFSBucket(database);

            Receive<RegisterRepository>(r => Context.ActorOf(Props.Create(() => new OperatorActor(repositoryData, gridFsBucket))).Forward(r));
        }
    }
}
