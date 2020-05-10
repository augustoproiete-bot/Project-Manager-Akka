using System.Diagnostics;
using Akka.Actor;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class GoStandByComponent : ReceiveActor
    {
        private readonly FanControlOptions _options;
        private readonly MessageBus _messageBus;
        private State _lastState = State.Idle;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public GoStandByComponent(FanControlOptions options, MessageBus messageBus)
        {
            _options = options;
            _messageBus = messageBus;

            messageBus.Subscribe<TrackingEvent>(Self);
            Receive<TrackingEvent>(Handle);
        }

        private void Handle(TrackingEvent msg)
        {
            if(msg.Error) return;

            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (_lastState)
                {
                    case State.StandBy when msg.State == State.StandBy:
                        if (_stopwatch.IsRunning && _stopwatch.Elapsed.Seconds < _options.GoStandbyTime)
                            _messageBus.Publish(new FanStartEvent());
                        else if (_stopwatch.IsRunning)
                            _stopwatch.Stop();
                        else
                            _stopwatch.Reset();
                        break;
                    case State.Power when msg.State == State.StandBy:
                        _messageBus.Publish(new FanStartEvent());
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