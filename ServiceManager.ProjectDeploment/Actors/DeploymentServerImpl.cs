using System;
using Akka.Actor;
using Akka.Routing;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class DeploymentServerImpl : ExposedReceiveActor
    {
        public const string AppsCollectionName = "Apps";

        public DeploymentServerImpl(IMongoClient client, IActorRef dataTransfer, RepositoryApi repositoryProxy)
        {
            var database = client.GetDatabase("Deployment");
            var trashBin = database.GetCollection<ToDeleteRevision>("TrashBin");
            var files = new GridFSBucket(database, new GridFSBucketOptions {BucketName = "Apps"});

            var cleanUp = Context.ActorOf(() => new CleanUpManager(database, "CleanUp", trashBin, files), "CleanUp-Manager");
            cleanUp.Tell(CleanUpManager.Initialization);

            var router = new SmallestMailboxPool(Environment.ProcessorCount)
                .WithSupervisorStrategy(Akka.Actor.SupervisorStrategy.DefaultStrategy);

            var queryProps = Props.Create(() => new AppQueryHandler(database.GetCollection<AppData>(AppsCollectionName, null), files, dataTransfer))
                .WithRouter(router);
            var query = Context.ActorOf(queryProps, "QueryRouter");

            Receive<IDeploymentQuery>(q => query.Forward(q));

            var buildSystem = WorkDistributor<BuildRequest, BuildCompled>.Create(Context, Props.Create<BuildingActor>(), "Compiler", TimeSpan.FromHours(1), "CompilerSupervisor");

            var processorProps = Props.Create(() => new AppCommandProcessor(database.GetCollection<AppData>(AppsCollectionName, null), files, dataTransfer, repositoryProxy, trashBin, buildSystem))
                .WithRouter(router);
            var processor = Context.ActorOf(processorProps, "ProcessorRouter");

            Receive<IDeploymentCommand>(a => processor.Forward(a));
        }
    }
}