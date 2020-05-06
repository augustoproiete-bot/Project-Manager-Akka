using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.Configuration;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl.Components
{
    public class StandByCoolingComponent : IHandler<TrackingEvent>
    {
        private readonly FanControlOptions _options;

        public StandByCoolingComponent(FanControlOptions options) => _options = options;

        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if(msg.Error) return;
            if(msg.State != State.StandBy || msg.Pt1000 < _options.MaxStandbyTemp) return;

            await messageBus.Publish(new FanStartEvent());
        }
    }
}