using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ServiceManager.ProjectRepository.Data
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