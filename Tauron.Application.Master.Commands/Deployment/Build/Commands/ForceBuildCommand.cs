using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class ForceBuildCommand : DeploymentCommandBase<FileTransactionId>
    {
        public IActorRef TransferManager { get; private set; } = ActorRefs.Nobody;
        public string Repository { get; private set; } = string.Empty;
        public string Project { get; private set; } = string.Empty;

        public ForceBuildCommand(IActorRef transferManager, string repository, string project) : base("Non")
        {
            TransferManager = transferManager;
            Repository = repository;
            Project = project;
        }

        public ForceBuildCommand([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        {
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            TransferManager = BinaryHelper.ReadRef(reader, system);
            Repository = reader.ReadString();
            Project = reader.ReadString();
            base.ReadInternal(reader, manifest, system);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteRef(writer, TransferManager);
            writer.Write(Repository);
            writer.Write(Project);
            base.WriteInternal(writer);
        }
    }
}