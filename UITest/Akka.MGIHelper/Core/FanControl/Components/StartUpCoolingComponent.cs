using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class StartUpCoolingComponent : ReceiveActor
    {
        private readonly FanControlOptions _options;
        private readonly MessageBus _eventStream;

        public StartUpCoolingComponent(FanControlOptions options, MessageBus eventStream)
        {
            _options = options;
            _eventStream = eventStream;

            eventStream.Subscribe<TrackingEvent>(Self);
            Receive<TrackingEvent>(Handle);
        }

        private void Handle(TrackingEvent msg)
        {
            if(msg.Error || msg.State != State.StartUp) return;

            if (msg.Pt1000 >= _options.MaxStartupTemp)
                _eventStream.Publish(new FanStartEvent());
        }
    }
}