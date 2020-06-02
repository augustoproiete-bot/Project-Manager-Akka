using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Components;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl
{
    public class FanControl : ReceiveActor
    {
        public FanControl(FanControlOptions options)
        {
            var parent = Context.Parent;

            var messageBus = new MessageBus();

            messageBus.Subscribe(new ClockComponent(options));

            messageBus.Subscribe(new DataFetchComponent(options));

            messageBus.Subscribe(new TrackingEventDeliveryComponent(e =>
            {
                parent.Tell(e);
                return Task.CompletedTask;
            }));

            messageBus.Subscribe(new PowerComponent());
            messageBus.Subscribe(new CoolDownComponent());
            messageBus.Subscribe(new GoStandByComponent(options));
            messageBus.Subscribe(new StartUpCoolingComponent(options));
            messageBus.Subscribe(new StandByCoolingComponent(options));

            messageBus.Subscribe(new FanControlComponent(options, e =>
            {
                parent.Tell(new FanStatusChange(e));
                return Task.CompletedTask;
            }));

            Receive<ClockEvent>(async msg => await messageBus.Publish(msg));
        }

        private void CreateComponent<TComp>(Expression<Func<TComp>> fac)
            where TComp : ActorBase
        {
            Context.ActorOf(Props.Create(fac), typeof(TComp).Name); //.WithDispatcher("akka.CallingThread")
        }
    }
}