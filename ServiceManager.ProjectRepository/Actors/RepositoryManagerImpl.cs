using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : ExposedReceiveActor, IWithTimers
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public RepositoryManagerImpl(IMongoClient client)
        {
            var dataTransfer = DataTransferManager.New(Context);

            var database = client.GetDatabase("Repository");

            var repositoryData = database.GetCollection<RepositoryEntry>("Repositorys");
            var trashBin = database.GetCollection<ToDeleteRevision>("TrashBin");

            if (!IsDefined(database.ListCollectionNames(), s => s == "CleanUp"))
                database.CreateCollection("CleanUp", new CreateCollectionOptions {Capped = true, MaxDocuments = 1, MaxSize = 1024});

            var cleanUp = database.GetCollection<CleanUpTime>("CleanUp");

            var gridFsBucket = new GridFSBucket(database, new GridFSBucketOptions
                                                          {
                                                              BucketName = "RepositoryData",
                                                              ChunkSizeBytes = 1048576
                                                          });

            Receive<RepositoryAction>(r => Context.ActorOf(Props.Create(() => new OperatorActor(repositoryData, gridFsBucket, trashBin, cleanUp, dataTransfer))).Forward(r));
            Receive<IndexRequest>(_ =>
            {
                try
                {
                    // ReSharper disable once VariableHidesOuterVariable
                    if (!IsDefined(repositoryData.Indexes.List(), _ => true))
                        repositoryData.Indexes.CreateOne(new CreateIndexModel<RepositoryEntry>(Builders<RepositoryEntry>.IndexKeys.Ascending(r => r.RepoName)));
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error on Building Repository index");
                }
            });

            Receive<InitCleanUp>(_ =>
            {
                var data = cleanUp.AsQueryable().FirstOrDefault();
                if (data == null)
                {
                    data = new CleanUpTime
                           {
                               Interval = TimeSpan.FromDays(7),
                               Last = DateTime.Now
                           };

                    cleanUp.InsertOne(data);
                }

                Timers.StartPeriodicTimer(data, new StartCleanUp(), TimeSpan.FromHours(1));
            });

            Receive<StartCleanUp>(r => Context.ActorOf(Props.Create(() => new OperatorActor(repositoryData, gridFsBucket, trashBin, cleanUp, dataTransfer))).Forward(r));
        }

        private bool IsDefined<TSource>(IAsyncCursor<TSource> cursor, Func<TSource, bool> predicate)
        {
            while (cursor.MoveNext())
            {
                if (cursor.Current.Any(predicate))
                    return true;
            }

            return false;
        }


        protected override void PreStart()
        {
            base.PreStart();
            Self.Tell(new IndexRequest());
            Self.Tell(new InitCleanUp());
        }

        private sealed class IndexRequest
        {

        }

        private sealed class InitCleanUp
        {

        }

        public ITimerScheduler Timers { get; set; } = null!;
    }
}
