using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class StandByCoolingComponent : ReceiveActor
    {
        private readonly FanControlOptions _options;
        private readonly MessageBus _messageBus;

        public StandByCoolingComponent(FanControlOptions options, MessageBus messageBus)
        {
            _options = options;
            _messageBus = messageBus;

            _messageBus.Subscribe<TrackingEvent>(Self);
            Receive<TrackingEvent>(Handle);
        }

        private void Handle(TrackingEvent msg)
        {
            if(msg.Error) return;
            if(msg.State != State.StandBy || msg.Pt1000 < _options.MaxStandbyTemp) return;

            _messageBus.Publish(new FanStartEvent());
        }
    }
}