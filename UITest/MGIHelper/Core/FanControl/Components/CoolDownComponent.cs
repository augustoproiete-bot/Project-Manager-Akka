using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl.Components
{
    public sealed class CoolDownComponent : IHandler<TrackingEvent>
    {
        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if(msg.Error) return;
            if (msg.State == State.Cooldown)
                await messageBus.Publish(new FanStartEvent());
        }
    }
}