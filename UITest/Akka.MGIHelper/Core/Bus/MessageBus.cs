using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;

namespace Akka.MGIHelper.Core.Bus
{
    public sealed class MessageBus
    {
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Type, List<IActorRef>> _handlers = new ConcurrentDictionary<Type, List<IActorRef>>();

        public IAsyncDisposable Subscribe<TMsg>(IActorRef handler)
        {
            var msgType = typeof(TMsg);
            lock (_lock)
            {
                if (_handlers.TryGetValue(msgType, out var list))
                {
                    list.Add(handler);
                }
                else
                    _handlers.TryAdd(msgType, new List<IActorRef>() {handler});
            }

            return new SubscribeDispose(handler, _handlers[msgType], _lock);
        }

        public void Publish<TMsg>(TMsg msg)
        {
            foreach (var handler in _handlers[typeof(TMsg)])
                handler.Tell(msg);
        }

        private sealed class SubscribeDispose : IAsyncDisposable
        {
            private readonly IActorRef _handler;
            private readonly List<IActorRef> _handlers;
            private readonly object _locker;

            public SubscribeDispose(IActorRef handler, List<IActorRef> handlers, object locker)
            {
                _handler = handler;
                _handlers = handlers;
                _locker = locker;
            }

            public ValueTask DisposeAsync()
            {
                lock (_locker)
                    _handlers.Remove(_handler);
                return new ValueTask(Task.CompletedTask);
            }
        }
    }
}