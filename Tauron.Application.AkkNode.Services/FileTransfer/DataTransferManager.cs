using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.FileTransfer.Operator;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    [PublicAPI]
    public sealed class DataTransferManager
    {
        public static DataTransferManager New(IActorRefFactory factory, string? name = null)
        {
            return new DataTransferManager(factory.ActorOf(Props.Create<DataTransferManagerActor>(), name));
        }

        public IActorRef Actor { get; }

        public DataTransferManager(IActorRef actor) => Actor = actor;
    }
}