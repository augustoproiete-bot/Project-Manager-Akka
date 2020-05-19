﻿using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Host
{
    [PublicAPI]
    public interface IHostLifetime
    {
        Task WaitForStartAsync(ActorSystem actorSystem);

        Task ShutdownTask { get; }
    }
}