using System;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Components;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl
{
    public class FanControl : ReceiveActor
    {
        public FanControl(FanControlOptions options)
        {
            var messageBus = new MessageBus();
            
            CreateComponent(() => new ClockComponent(options, messageBus));

            CreateComponent(() => new DataFetchComponent(options, messageBus));

            CreateComponent(() => new PowerComponent(messageBus));
            CreateComponent(() => new CoolDownComponent(messageBus));
            CreateComponent(() => new GoStandByComponent(options, messageBus));
            CreateComponent(() => new StartUpCoolingComponent(options, messageBus));
            CreateComponent(() => new StandByCoolingComponent(options, messageBus));

            CreateComponent(() => new FanControlComponent(options, messageBus));

            messageBus.Subscribe<TrackingEvent>(Context.Parent);
            messageBus.Subscribe<FanStatusChange>(Context.Parent);

            Receive<ClockEvent>(msg => messageBus.Publish(msg));
        }

        private void CreateComponent<TComp>(Expression<Func<TComp>> fac) 
            where TComp : ActorBase =>
            Context.ActorOf(Props.Create(fac), typeof(TComp).Name); //.WithDispatcher("akka.CallingThread")
    }
}