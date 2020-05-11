using System;
using System.Threading.Tasks;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.Core.FanControl.Components
{
    public class TrackingEventDeliveryComponent : IHandler<TrackingEvent>
    {
        private readonly Func<TrackingEvent, Task> _invoker;

        public TrackingEventDeliveryComponent(Func<TrackingEvent, Task> invoker) => _invoker = invoker;

        public Task Handle(TrackingEvent msg, MessageBus messageBus) => _invoker(msg);
    }
}