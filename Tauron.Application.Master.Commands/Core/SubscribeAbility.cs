using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Master.Commands.Core
{
    [PublicAPI]
    public sealed class SubscribeAbility
    {
        private readonly ExposedReceiveActor _actor;

        public event Action<Terminated>? Terminated;

        public SubscribeAbility(ExposedReceiveActor actor) 
            => _actor = actor;

        public void MakeReceive()
        {

        }
    }
}