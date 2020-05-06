using System;
using System.Threading;
using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.Configuration;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl.Components
{
    public sealed class ClockComponent : IHandler<ClockEvent>, IAsyncDisposable
    {
        private readonly FanControlOptions _options;
        private readonly Timer _timer;

        private MessageBus _messageBus;
        private ClockState _clockState;

        public ClockComponent(FanControlOptions options)
        {
            _options = options;
            _timer = new Timer(Invoke);
        }

        private async void Invoke(object state)
        {
            try
            {
                await _messageBus.Publish(new TickEvent());
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (_clockState == ClockState.Start)
                {
                    _timer.Change(_options.ClockTimeMs, -1);
                }
            }
        }

        public Task Handle(ClockEvent msg, MessageBus messageBus)
        {
            _messageBus = messageBus;
            _clockState = msg.ClockState;

            switch (msg.ClockState)
            {
                case ClockState.Start:
                    _timer.Change(_options.ClockTimeMs, -1);
                    break;
                case ClockState.Stop:
                    _timer.Change(-1, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync() => await _timer.DisposeAsync();
    }
}