using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class StandByCoolingComponent : IHandler<TrackingEvent>
    {
        private readonly FanControlOptions _options;

        public StandByCoolingComponent(FanControlOptions options)
        {
            _options = options;
        }

        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if (msg.Error) return;
            if (msg.State != State.StandBy || msg.Pt1000 < _options.MaxStandbyTemp) return;

            await messageBus.Publish(new FanStartEvent());
        }
    }
}