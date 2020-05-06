using System;
using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl.Components
{
    public class TrackingEventDeliveryComponent : IHandler<TrackingEvent>
    {
        private readonly Func<TrackingEvent, Task> _invoker;

        public TrackingEventDeliveryComponent(Func<TrackingEvent, Task> invoker) => _invoker = invoker;

        public Task Handle(TrackingEvent msg, MessageBus messageBus) => _invoker(msg);
    }
}