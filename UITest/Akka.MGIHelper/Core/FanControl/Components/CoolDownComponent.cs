using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public sealed class CoolDownComponent : ReceiveActor
    {
        private readonly MessageBus _eventStream;

        public CoolDownComponent(MessageBus eventStream)
        {
            _eventStream = eventStream;

            _eventStream.Subscribe<TrackingEvent>(Self);
            Receive<TrackingEvent>(Handle);
        }

        private void Handle(TrackingEvent msg)
        {
            if(msg.Error) return;
            if (msg.State == State.Cooldown)
                _eventStream.Publish(new FanStartEvent());
        }
    }
}