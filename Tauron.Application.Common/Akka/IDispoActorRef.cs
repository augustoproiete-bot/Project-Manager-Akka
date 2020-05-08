using System;
using Akka.Actor;

namespace Tauron.Akka
{
    public interface IDispoActorRef<TActor> : IActorRef, IDisposable
    {
        
    }
}