using System.Diagnostics;
using System.Threading.Tasks;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class GoStandByComponent : IHandler<TrackingEvent>
    {
        private readonly FanControlOptions _options;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private State _lastState = State.Idle;

        public GoStandByComponent(FanControlOptions options)
        {
            _options = options;
        }

        public async Task Handle(TrackingEvent msg, MessageBus messageBus)
        {
            if (msg.Error) return;

            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (_lastState)
                {
                    case State.StandBy when msg.State == State.StandBy:
                        if (_stopwatch.IsRunning && _stopwatch.Elapsed.Seconds < _options.GoStandbyTime)
                            await messageBus.Publish(new FanStartEvent());
                        else if (_stopwatch.IsRunning)
                            _stopwatch.Stop();
                        else
                            _stopwatch.Reset();
                        break;
                    case State.Power when msg.State == State.StandBy:
                        await messageBus.Publish(new FanStartEvent());
                        _stopwatch.Start();
                        break;
                }
            }
            finally
            {
                _lastState = msg.State;
            }
        }
    }
}