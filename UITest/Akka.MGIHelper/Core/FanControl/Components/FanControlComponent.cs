using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class FanControlComponent : IHandler<FanStartEvent>, IDisposable
    {
        private readonly FanControlOptions _options;
        private readonly Func<bool, Task> _statusChange;
        private readonly Timer _timer;

        public FanControlComponent(FanControlOptions options, Func<bool, Task> statusChange)
        {
            _timer = new Timer(StopFan);
            _options = options;
            _statusChange = statusChange;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public async Task Handle(FanStartEvent msg, MessageBus messageBus)
        {
            //TODO Start Fan

            await _statusChange(true);

            var time = (int) (_options.ClockTimeMs * _options.FanControlMultipler);

            _timer.Change(time, -1);
        }

        private async void StopFan(object state)
        {
            //TODO Stop Fan

            await _statusChange(false);
        }
    }
}