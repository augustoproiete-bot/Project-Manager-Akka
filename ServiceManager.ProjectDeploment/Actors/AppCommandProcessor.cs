using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Akka;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ExposedReceiveActor
    {
        public AppCommandProcessor(IMongoCollection<AppData> apps, GridFSBucket files, IActorRef dataTransfer)
        {
            
        }
    }
}