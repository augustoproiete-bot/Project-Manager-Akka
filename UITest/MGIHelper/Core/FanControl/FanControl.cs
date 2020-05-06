using System;
using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.Configuration;
using MGIHelper.Core.FanControl.Components;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl
{
    public class FanControl : IAsyncDisposable
    {
        public event Func<TrackingEvent, Task> DataRecieved;

        public event Func<bool, Task> FanStatus; 

        public MessageBus MessageBus { get; } = new MessageBus();

        public FanControl(FanControlOptions options)
        {
            MessageBus.Subscribe(new ClockComponent(options));

            MessageBus.Subscribe(new DataFetchComponent(options));

            MessageBus.Subscribe(new TrackingEventDeliveryComponent(e => DataRecieved?.Invoke(e) ?? Task.CompletedTask));

            MessageBus.Subscribe(new PowerComponent());
            MessageBus.Subscribe(new CoolDownComponent());
            MessageBus.Subscribe(new GoStandByComponent(options));
            MessageBus.Subscribe(new StartUpCoolingComponent(options));
            MessageBus.Subscribe(new StandByCoolingComponent(options));

            MessageBus.Subscribe(new FanControlComponent(options, e => FanStatus?.Invoke(e)));
        }

        public async void Start() => await MessageBus.Publish(new ClockEvent(ClockState.Start));

        public ValueTask DisposeAsync() 
            => MessageBus.DisposeAsync();
    }
}