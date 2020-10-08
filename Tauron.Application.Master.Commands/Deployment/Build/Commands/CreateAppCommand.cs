using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed class CreateAppCommand : DeploymentCommandBase<AppInfo>
    {
        public string TargetRepo { get; private set; } = string.Empty;

        public CreateAppCommand(string appName, string targetRepo)
            : base(appName)
            => TargetRepo = targetRepo;

        public CreateAppCommand([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        { }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            TargetRepo = reader.ReadString();
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(TargetRepo);
            base.WriteInternal(writer);
        }
    }
}