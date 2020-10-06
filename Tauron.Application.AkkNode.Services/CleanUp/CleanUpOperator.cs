using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class CleanUpOperator : ExposedReceiveActor
    {
        public CleanUpOperator(IMongoDatabase database, GridFSBucket bucked)
        {
            
        }
    }
}