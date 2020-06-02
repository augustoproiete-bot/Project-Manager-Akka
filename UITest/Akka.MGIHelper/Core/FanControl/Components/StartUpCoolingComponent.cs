using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class StartUpCoolingComponent : IHandler<TrackingEvent>
    {
        private readonly FanControlOptions _options;

        public StartUpCoolingComponent(FanControlOptions options)
        {
            _options = options;
        }

        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if (msg.Error || msg.State != State.StartUp) return;

            if (msg.Pt1000 >= _options.MaxStartupTemp)
                await messageBus.Publish(new FanStartEvent());
        }
    }
}