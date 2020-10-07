using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    internal sealed class RepositoryManagerImpl : ExposedReceiveActor, IWithTimers
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public RepositoryManagerImpl(IMongoClient client, IActorRef dataTransfer)
        {
            var database = client.GetDatabase("Repository");

            var repositoryData = database.GetCollection<RepositoryEntry>("Repositorys");
            var trashBin = database.GetCollection<ToDeleteRevision>("TrashBin");

            var gridFsBucket = new GridFSBucket(database, new GridFSBucketOptions
                                                          {
                                                              BucketName = "RepositoryData",
                                                              ChunkSizeBytes = 1048576
                                                          });

            Receive<RepositoryAction>(r => Context.ActorOf(Props.Create(() => new OperatorActor(repositoryData, gridFsBucket, trashBin, dataTransfer))).Forward(r));
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

            var cleanUp = Context.ActorOf(Props.Create(() => new CleanUpManager(database, "CleanUp", trashBin, gridFsBucket)), "CleanUp-Manager");
            cleanUp.Tell(CleanUpManager.Initialization);

            Receive<StartCleanUp>(c => cleanUp.Forward(c));
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
        }


        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(e => Directive.Stop);

        private sealed class IndexRequest
        {

        }



        public ITimerScheduler Timers { get; set; } = null!;
    }
}
