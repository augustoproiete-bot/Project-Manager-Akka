using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl.Components
{
    public class PowerComponent : IHandler<TrackingEvent>
    {
        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if (msg.State == State.Power)
                await messageBus.Publish(new FanStartEvent());
        }
    }
}