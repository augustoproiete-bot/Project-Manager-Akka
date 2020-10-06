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

        public CleanUpManager(IMongoDatabase database,  string clenUpCollection, IMongoCollection<ToDeleteRevision> revisions, GridFSBucket bucked)
        {
            bool IsDefined<TSource>(IAsyncCursor<TSource> cursor, Func<TSource, bool> predicate)
            {
                while (cursor.MoveNext())
                {
                    if (cursor.Current.Any(predicate))
                        return true;
                }

                return false;
            }

            Receive<InitCleanUp>(_ =>
            {
                if (!IsDefined(database.ListCollectionNames(), s => s == "CleanUp"))
                    database.CreateCollection("CleanUp", new CreateCollectionOptions { Capped = true, MaxDocuments = 1, MaxSize = 1024 });

                var data = database.AsQueryable().FirstOrDefault();
                if (data == null)
                {
                    data = new CleanUpTime
                           {
                               Interval = TimeSpan.FromDays(7),
                               Last = DateTime.Now
                           };

                    database.InsertOne(data);
                }

                Timers.StartPeriodicTimer(data, new StartCleanUp(), TimeSpan.FromHours(1));
            });

            Receive<StartCleanUp>(r => Context.ActorOf(Props.Create(() => new CleanUpOperator(database, revisions, bucked))).Forward(r));
        }


        public sealed class InitCleanUp
        {
            internal InitCleanUp()
            {
                
            }
        }
    }
}