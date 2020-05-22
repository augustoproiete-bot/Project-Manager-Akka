using System;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Workspace.MutatingEngine;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public static class EventSourceExtensions
    {
        public static void RespondOnEventSource<TData>(this ExposedReceiveActor actor, IEventSource<TData> eventSource, Action<TData> action)
        {
            eventSource.RespondOn(actor.ExposedContext.Self);
            actor.Exposed.Receive<TData>((data, context) => action(data));
        }
    }
}
