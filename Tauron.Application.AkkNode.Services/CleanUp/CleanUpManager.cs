using System;
using System.Linq;
using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class CleanUpManager : ExposedReceiveActor, IWithTimers
    {
        public static readonly InitCleanUp Initialization = new InitCleanUp();

        public ITimerScheduler Timers { get; set; } = null!;

        public CleanUpManager(IMongoDatabase database, string cleanUpCollection, IMongoCollection<ToDeleteRevision> revisions, GridFSBucket bucked)
        {


            void Initializing()
            {
                Receive<InitCleanUp>(_ =>
                {
                    if (!database.ListCollectionNames().Contains(s => s == cleanUpCollection))
                        database.CreateCollection("CleanUp", new CreateCollectionOptions {Capped = true, MaxDocuments = 1, MaxSize = 1024});

                    var cleanUp = database.GetCollection<CleanUpTime>(cleanUpCollection);

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

                    Become(Running);
                });
            }

            void Running()
            {
                Receive<StartCleanUp>(r => Context.ActorOf(Props.Create(() => new CleanUpOperator(database.GetCollection<CleanUpTime>(cleanUpCollection, null), revisions, bucked))).Forward(r));
            }

            Become(Initializing);
        }


        public sealed class InitCleanUp
        {
            internal InitCleanUp()
            {
                
            }
        }
    }
}