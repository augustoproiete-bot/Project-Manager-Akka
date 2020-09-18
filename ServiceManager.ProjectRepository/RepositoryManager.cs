using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectRepository.Actors;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public sealed class RepositoryManager
    {
        public static readonly RepositoryManager Empty = new RepositoryManager(ActorRefs.Nobody); 

        public static RepositoryManager CreateInstance(IActorRefFactory factory, string connectionString)
            => new RepositoryManager(factory.ActorOf(Props.Create(() => new RepositoryManagerImpl(new MongoClient(connectionString)))));

        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, string connectionString) 
            => InitRepositoryManager(actorSystem, new MongoClient(connectionString));

        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, IMongoClient client)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new RepositoryManagerImpl(client)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return new RepositoryManager(actorSystem.ActorOf(repo, RepositoryApi.RepositoryPath));
        }

        private readonly IActorRef _manager;

        private RepositoryManager(IActorRef manager) => _manager = manager;

        public void CleanUp()
            => _manager.Tell(new StartCleanUp());

        public void SendAction(RepositoryAction action)
            => _manager.Tell(action);
    }
}