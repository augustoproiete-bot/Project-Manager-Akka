using System;
using Akka.Actor;
using Akka.Event;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : ExposedReceiveActor, IWithTimers
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<ToDeleteRevision> _trashBin;
        private readonly GridFSBucket _gridFsBucket;

        public RepositoryManagerImpl(IMongoClient client, DataTransferManager dataTransfer)
        {
            _database = client.GetDatabase("Repository");

            var repositoryData = _database.GetCollection<RepositoryEntry>("Repositorys");
            _trashBin = _database.GetCollection<ToDeleteRevision>("TrashBin");

            _gridFsBucket = new GridFSBucket(_database, new GridFSBucketOptions
                                                          {
                                                              BucketName = "RepositoryData",
                                                              ChunkSizeBytes = 1048576
                                                          });

            Receive<IRepositoryAction>(r => Context.ActorOf(Props.Create(() => new OperatorActor(repositoryData, _gridFsBucket, _trashBin, dataTransfer))).Forward(r));
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

            IActorRef cleaner = CreateCleanUp();

            Receive<Terminated>(t =>
            {
                if (t.ActorRef.Equals(cleaner))
                    cleaner = CreateCleanUp();
            });

            Context.Watch(cleaner);

            Receive<StartCleanUp>(c => cleaner.Forward(c));
        }

        private IActorRef CreateCleanUp()
        {
            var cleanUp = Context.ActorOf(Props.Create(() => new CleanUpManager(_database, "CleanUp", _trashBin, _gridFsBucket)), "CleanUp-Manager");
            cleanUp.Tell(CleanUpManager.Initialization);

            return cleanUp;
        }

        protected override void PreStart()
        {
            base.PreStart();
            Self.Tell(new IndexRequest());
        }


        protected override SupervisorStrategy SupervisorStrategy() => Akka.Actor.SupervisorStrategy.StoppingStrategy;

        private sealed class IndexRequest
        {

        }



        public ITimerScheduler Timers { get; set; } = null!;
    }
}
