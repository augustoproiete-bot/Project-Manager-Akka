using System;
using Akka.Actor;
using Akka.Event;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : RepositoryManager
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public RepositoryManagerImpl(IMongoClient client)
        {
            var database = client.GetDatabase("Repository");

            var repositoryData = database.GetCollection<RepositoryEntry>("Repositorys");
            var gridFsBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = "RepositoryData",
                ChunkSizeBytes = 1048576
            });

            Receive<RepositoryAction>(r => Context.ActorOf(Props.Create(() => new OperatorActor(repositoryData, gridFsBucket))).Forward(r));
            Receive<IndexRequest>(_ =>
            {
                try
                {
                    repositoryData.Indexes.CreateOne(new CreateIndexModel<RepositoryEntry>(Builders<RepositoryEntry>.IndexKeys.Ascending(r => r.RepoName)));
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error on Building Repository index");
                }
            });
        }

        protected override void PreStart()
        {
            base.PreStart();
            Self.Tell(new IndexRequest());
        }

        private sealed class IndexRequest
        {
            
        }
    }
}
