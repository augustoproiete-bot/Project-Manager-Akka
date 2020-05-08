﻿using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IEventActor
    {
        void Register(HookEvent hookEvent);
        
        void Send(IActorRef actor, object send);
    }
}