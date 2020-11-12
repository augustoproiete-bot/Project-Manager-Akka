using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public abstract class SharedEvent<TPayload>
    {
        private readonly WeakActionEvent<TPayload> _handlerList = new();

        public virtual void Publish(TPayload content) 
            => _handlerList.Invoke(content);

        public void Subscribe(Action<TPayload> handler) 
            => _handlerList.Add(Argument.NotNull(handler, nameof(handler)));

        public void UnSubscribe(Action<TPayload> handler) 
            => _handlerList.Remove(Argument.NotNull(handler, nameof(handler)));
    }

    [PublicAPI]
    public interface IEventAggregator
    {
        TEventType GetEvent<TEventType, TPayload>() where TEventType : SharedEvent<TPayload>, new();
    }

    [PublicAPI]
    public sealed class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, object> _events = new();

        public TEventType GetEvent<TEventType, TPayload>() where TEventType : SharedEvent<TPayload>, new()
        {
            var t = typeof(TEventType);
            if (!_events.ContainsKey(t)) _events[t] = new TEventType();

            return (TEventType) _events[t];
        }
    }
}