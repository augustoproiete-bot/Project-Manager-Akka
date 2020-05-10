using System.Threading;
using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class FanControlComponent : ReceiveActor
    {
        private readonly Timer _timer;

        private readonly FanControlOptions _options;
        private readonly MessageBus _messageBus;

        public FanControlComponent(FanControlOptions options, MessageBus messageBus)
        {
            _timer = new Timer(StopFan);
            _options = options;
            _messageBus = messageBus;

            messageBus.Subscribe<FanStartEvent>(Self);
            Receive<FanStartEvent>(Handle);
        }

        private void Handle(FanStartEvent msg)
        {
            //TODO Start Fan

            _messageBus.Publish(new FanStatusChange(true));

            var time = (int) (_options.ClockTimeMs * _options.FanControlMultipler);

            _timer.Change(time, -1);
        }

        private void StopFan(object? state)
        {
            //TODO Stop Fan

            _messageBus.Publish(new FanStatusChange(false));
        }

        protected override void PostStop() 
            => _timer.Dispose();
    }
}