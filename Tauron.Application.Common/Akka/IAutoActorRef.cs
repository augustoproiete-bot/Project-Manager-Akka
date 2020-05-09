﻿using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IAutoActorRef<TActor> : IActorRef, IInitableActorRef
    {
    }
}