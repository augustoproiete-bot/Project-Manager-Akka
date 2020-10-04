using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class EventSource<TRespond, TData> : EventSourceBase<TRespond>
    {
        public EventSource(Task<IActorRef> mutator, Func<TData, TRespond> transform, Func<TData, bool>? where, IRespondHandler<TData> handler)
            : base(mutator)
        {
            if (where == null)
                handler.Register(d => Send(transform(d)));
            else
                handler.Register(d =>
                {
                    if (@where(d))
                        Send(transform(d));
                });
        }
    }
}