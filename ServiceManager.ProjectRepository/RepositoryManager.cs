using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectRepository.Actors;
using Tauron.Akka;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository
{
    [PublicAPI]
    public abstract class RepositoryManager : ExposedReceiveActor
    {
        public static IActorRef InitRepositoryManager(ActorSystem actorSystem, string connectionString)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new RepositoryManagerImpl(new MongoClient(connectionString))),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return actorSystem.ActorOf(repo, RepositoryApi.RepositoryPath);
        }

        public static IActorRef InitRepositoryManager(ActorSystem actorSystem, IMongoClient client)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new RepositoryManagerImpl(client)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return actorSystem.ActorOf(repo, RepositoryApi.RepositoryPath);
        }
    }
}