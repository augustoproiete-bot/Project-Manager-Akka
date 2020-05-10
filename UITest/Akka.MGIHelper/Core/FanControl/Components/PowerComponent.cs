using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class PowerComponent : ReceiveActor
    {
        private readonly MessageBus _messageBus;

        public PowerComponent(MessageBus messageBus)
        {
            _messageBus = messageBus;

            _messageBus.Subscribe<TrackingEvent>(Self);
            Receive<TrackingEvent>(Handle);
        }

        private void Handle(TrackingEvent msg)
        {
            if (msg.State == State.Power)
                _messageBus.Publish(new FanStartEvent());
        }
    }
}