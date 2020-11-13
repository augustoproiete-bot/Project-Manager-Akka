using System;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public interface IEventSourceable<TData>
    {
        IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer, Func<Maybe<TData>, bool> where);

        IEventSource<TRespond> EventSource<TRespond>(Func<Maybe<TData>, Maybe<TRespond>> transformer);
    }
}