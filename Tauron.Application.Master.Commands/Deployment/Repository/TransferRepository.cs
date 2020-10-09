using System.IO;
using Akka.Actor;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed class TransferRepository : RepositoryAction
    {
        public IActorRef FileTarget { get; private set; } = ActorRefs.Nobody;

        public string OperationId { get; private set; } = string.Empty;

        public TransferRepository(string repoName, IActorRef listner, IActorRef fileTarget, string operationId)
            : base(repoName, listner)
        {
            FileTarget = fileTarget;
            OperationId = operationId;
        }
        
        public TransferRepository(BinaryReader reader, ExtendedActorSystem system)
            : base(reader, system) { }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            FileTarget = BinaryHelper.ReadRef(reader, system);
            OperationId = reader.ReadString();
            base.ReadInternal(reader, manifest, system);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteRef(writer, FileTarget);
            writer.Write(OperationId);
            base.WriteInternal(writer);
        }
    }
}