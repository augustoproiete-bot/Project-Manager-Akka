using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectRepository.Actors;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public sealed class RepositoryManager
    {
        public static readonly RepositoryManager Empty = new RepositoryManager(ActorRefs.Nobody); 

        public static RepositoryManager CreateInstance(IActorRefFactory factory, string connectionString, IActorRef tranferManager)
            => new RepositoryManager(factory.ActorOf(Props.Create(() => new RepositoryManagerImpl(new MongoClient(connectionString), tranferManager))));

        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, string connectionString, IActorRef tranferManager) 
            => InitRepositoryManager(actorSystem, new MongoClient(connectionString), tranferManager);

        public static RepositoryManager InitRepositoryManager(ActorSystem actorSystem, IMongoClient client, IActorRef tranferManager)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new RepositoryManagerImpl(client, tranferManager)),
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