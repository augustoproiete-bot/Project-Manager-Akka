using System;
using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Workspace.MutatingEngine
{
    public sealed class EventSource<TRespond, TData> : EventSourceBase<TRespond>
    {
        public EventSource(IActorRef mutator, Func<TData, TRespond> transform, Func<TData, bool>? where, IRespondHandler<TData> handler) 
            : base(mutator)
        {
            if (where == null)
                handler.Register(d => Send(transform(d)));
            else
            {
                handler.Register(d =>
                                 {
                                     if(where(d))
                                        Send(transform(d));
                                 });
            }
        }
    }
}