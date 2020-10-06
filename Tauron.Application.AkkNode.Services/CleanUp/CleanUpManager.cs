using System;
using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class CleanUpManager : ExposedReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; } = null!;

        public CleanUpManager(IMongoDatabase database, GridFSBucket bucked)
        {
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

            Receive<StartCleanUp>(r => Context.ActorOf(Props.Create(() => new CleanUpOperator(database, bucked))).Forward(r));
        }

        protected override void PreStart()
        {
            Self.Tell(new InitCleanUp());
            base.PreStart();
        }

        private sealed class InitCleanUp
        {

        }
    }
}