using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;
using Tauron.Host;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public sealed class ClockComponent : IHandler<ClockEvent>, IAsyncDisposable
    {
        private readonly FanControlOptions _options;
        private readonly Timer _timer;
        private ClockState _clockState;

        private MessageBus? _messageBus;

        public ClockComponent(FanControlOptions options)
        {
            _options = options;
            ActorApplication.Application.ActorSystem.RegisterOnTermination(() => _timer.Change(-1, -1));
            _timer = new Timer(Invoke);
        }

        public async ValueTask DisposeAsync()
        {
            await _timer.DisposeAsync();
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

        private async void Invoke(object state)
        {
            try
            {
                await _messageBus!.Publish(new TickEvent());
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (_clockState == ClockState.Start) _timer.Change(_options.ClockTimeMs, -1);
            }
        }
    }
}