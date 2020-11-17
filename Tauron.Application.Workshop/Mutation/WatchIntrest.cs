using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.Mutation
{
    public sealed record WatchIntrest(IActorRef Target, Action OnRemove);
}