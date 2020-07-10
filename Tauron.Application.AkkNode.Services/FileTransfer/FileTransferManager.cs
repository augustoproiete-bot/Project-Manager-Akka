using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.FileTransfer.Operator;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public static class FileTransferManager
    {
        public static IActorRef New(IActorRefFactory factory)
            => factory.ActorOf(Props.Create<FileTransferManagerActor>())
    }
}