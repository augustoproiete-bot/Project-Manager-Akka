using System;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IEventSourceable<TData>
    {
        IEventSource<TRespond> EventSource<TRespond>(Func<TData, TRespond> transformer, Func<TData, bool>? where = null);
    }
}