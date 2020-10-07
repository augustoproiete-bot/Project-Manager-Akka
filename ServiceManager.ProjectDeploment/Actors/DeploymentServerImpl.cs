using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.CleanUp;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class DeploymentServerImpl : ExposedReceiveActor
    {
        public DeploymentServerImpl(IMongoClient client)
        {
            var database = client.GetDatabase("Deploment");
            var trashBin = database.GetCollection<ToDeleteRevision>("TrashBin");
            var files = new GridFSBucket(database, new GridFSBucketOptions {BucketName = "Apps"});

            var cleanUp = Context.ActorOf(() => new CleanUpManager(database, "CleanUp", trashBin, files), "CleanUp-Manager");
            cleanUp.Tell(CleanUpManager.Initialization);


        }
    }
}