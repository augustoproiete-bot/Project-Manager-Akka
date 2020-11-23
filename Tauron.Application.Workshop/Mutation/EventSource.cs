using System;
using System.Threading.Tasks;
using Akka.Actor;
using Functional.Maybe;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed class EventSource<TRespond, TData> : EventSourceBase<TRespond>
    {
        public EventSource(WorkspaceSuperviser superviser, Task<IActorRef> mutator, Func<Maybe<TData>, Maybe<TRespond>> transform, Maybe<Func<Maybe<TData>, Maybe<bool>>> where, IRespondHandler<TData> handler)
            : base(mutator, superviser)
        {
            where.Match
            (
                w =>
                {
                    handler.Register(d =>
                    {
                        if (w(d).OrElse(false))
                            Send(transform(d));
                    });
                },
                () => handler.Register(d => Send(transform(d)))
            );
        }
    }
}