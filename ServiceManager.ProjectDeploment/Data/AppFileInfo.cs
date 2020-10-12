using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed class AppFileInfo
    {
        [BsonElement]
        public ObjectId File { get; }

        [BsonElement]
        public int Version { get; }

        [BsonElement]
        public DateTime CreationTime { get; }

        [BsonElement]
        public bool Deleted { get; set; }

        [BsonElement]
        public string Commit { get; }

        [BsonConstructor]
        public AppFileInfo(ObjectId file, int version, DateTime creationTime, bool deleted, string commit)
        {
            File = file;
            Version = version;
            CreationTime = creationTime;
            Deleted = deleted;
            Commit = commit;
        }
    }
}