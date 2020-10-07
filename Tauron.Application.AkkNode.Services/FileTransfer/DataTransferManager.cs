﻿using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.FileTransfer.Operator;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public static class DataTransferManager
    {
        public static IActorRef New(IActorRefFactory factory, string? name = null)
            => factory.ActorOf(Props.Create<DataTransferManagerActor>(), name);
    }
}