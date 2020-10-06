using MongoDB.Driver;
using Tauron.Akka;

namespace ServiceManager.ProjectDeploment.Actors
{
    public sealed class DeplomentServerImpl : ExposedReceiveActor
    {
        public DeplomentServerImpl(IMongoClient database)
        {
            
        }
    }
}