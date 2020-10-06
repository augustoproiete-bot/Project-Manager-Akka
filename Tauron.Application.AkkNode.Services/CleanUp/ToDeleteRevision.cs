using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class ToDeleteRevision
    {
        public ToDeleteRevision(string current) => BuckedId = current;

        public ToDeleteRevision()
        {
            
        }

        [BsonId]
        public ObjectId Id { get; set; }

        public string BuckedId { get; set; } = string.Empty;
    }
}