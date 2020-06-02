using System.Threading.Tasks;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public sealed class CoolDownComponent : IHandler<TrackingEvent>
    {
        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if (msg.Error) return;
            if (msg.State == State.Cooldown)
                await messageBus.Publish(new FanStartEvent());
        }
    }
}