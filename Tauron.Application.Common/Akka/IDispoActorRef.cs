using System;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IDispoActorRef<TActor> : IActorRef, IDisposable, IInitableActorRef
    {
        void Init(string? name = null);

        void Init(IActorRefFactory factory, string? name = null);
    }
}