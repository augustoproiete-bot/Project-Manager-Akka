using System;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using Akka.MGIHelper.Core.Bus;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public sealed class ClockComponent : ReceiveActor
    {
        private sealed class ActionTimer : IDisposable
        {
            private readonly Action _target;
            private readonly FanControlOptions _options;
            private readonly Timer _timer;

            private ClockState _clockState = ClockState.Stop;

            public ActionTimer(Action target, FanControlOptions options)
            {
                _target = target;
                _options = options;
                _timer = new Timer(RunTimer);
            }

            private void RunTimer(object? state) 
                => _target();

            public void Change(ClockState? state)
            {
                if(state != null)
                    _clockState = state.Value;
                if (state == ClockState.Start)
                    _timer.Change(_options.ClockTimeMs, -1);
            }

            public void Dispose() 
                => _timer.Dispose();
        }

        private readonly ActionTimer _timer;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly MessageBus _messageBus;

        public ClockComponent(FanControlOptions options, MessageBus eventStream)
        {
            _messageBus = eventStream;

            eventStream.Subscribe<ClockEvent>(Self);

            Receive<ClockEvent>(Handle);

            _timer = new ActionTimer(Invoke, options);
        }


        private void Invoke() => _messageBus.Publish(new TickEvent());

        private void Handle(ClockEvent msg)
        {
            _log.Info("Change Clock State: {State}", msg.ClockState);

            _timer.Change(msg.ClockState);
        }
    }
}