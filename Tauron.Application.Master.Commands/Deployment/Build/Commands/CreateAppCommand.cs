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

        public string ProjectName { get; private set; } = string.Empty; 

        public CreateAppCommand(string appName, string targetRepo, string projectName)
            : base(appName)
        {
            TargetRepo = targetRepo;
            ProjectName = projectName;
        }

        public CreateAppCommand([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        {
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            ProjectName = reader.ReadString();
            TargetRepo = reader.ReadString();
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(ProjectName);
            writer.Write(TargetRepo);
            base.WriteInternal(writer);
        }
    }
}