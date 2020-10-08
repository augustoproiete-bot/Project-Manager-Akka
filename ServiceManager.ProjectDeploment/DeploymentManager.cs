using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using JetBrains.Annotations;
using MongoDB.Driver;
using ServiceManager.ProjectDeployment.Actors;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment
{
    [PublicAPI]
    public sealed class DeploymentManager
    {
        public static readonly DeploymentManager Empty = new DeploymentManager(ActorRefs.Nobody); 

        public static DeploymentManager CreateInstance(IActorRefFactory factory, string connectionString, IActorRef manager, RepositoryApi api)
            => new DeploymentManager(factory.ActorOf(Props.Create(() => new DeploymentServerImpl(new MongoClient(connectionString), manager, api))));

        public static DeploymentManager InitDeploymentManager(ActorSystem actorSystem, string connectionString, IActorRef manager, RepositoryApi api) 
            => InitDeploymentManager(actorSystem, new MongoClient(connectionString), manager, api);

        public static DeploymentManager InitDeploymentManager(ActorSystem actorSystem, IMongoClient client, IActorRef manager, RepositoryApi api)
        {
            var repo = ClusterSingletonManager.Props(Props.Create(() => new DeploymentServerImpl(client, manager, api)),
                ClusterSingletonManagerSettings.Create(actorSystem).WithRole("UpdateSystem"));
            return new DeploymentManager(actorSystem.ActorOf(repo, DeploymentApi.RepositoryPath));
        }

        private readonly IActorRef _manager;

        private DeploymentManager(IActorRef manager) => _manager = manager;

        public void SendAction(DeplaymentAction action)
            => _manager.Tell(action);
    }
}