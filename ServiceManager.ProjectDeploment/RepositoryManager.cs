using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectDeploment.Actors;
using Tauron.Application.Master.Commands.Deployment.Deployment;

namespace ServiceManager.ProjectDeploment
{
    [PublicAPI]
    public sealed class DeplomentManager
    {
        public static readonly DeplomentManager Empty = new DeplomentManager(ActorRefs.Nobody); 

        public static DeplomentManager CreateInstance(IActorRefFactory factory, string connectionString)
            => new DeplomentManager(factory.ActorOf(Props.Create(() => new DeplomentServerImpl(new MongoClient(connectionString)))));

        public static DeplomentManager InitRepositoryManager(ActorSystem actorSystem, string connectionString) 
            => InitRepositoryManager(actorSystem, new MongoClient(connectionString));

        public static DeplomentManager InitRepositoryManager(ActorSystem actorSystem, IMongoClient client)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new DeplomentServerImpl(client)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return new DeplomentManager(actorSystem.ActorOf(repo, DeploymentApi.RepositoryPath));
        }

        private readonly IActorRef _manager;

        private DeplomentManager(IActorRef manager) => _manager = manager;

        public void SendAction(DeplaymentAction action)
            => _manager.Tell(action);
    }
}