using System.IO;
using Akka.Actor;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed class RegisterRepository : RepositoryAction
    {
        public bool IgnoreDuplicate { get; set; } = false;

        public RegisterRepository(string repoName, IActorRef listner)
            : base(repoName, listner)
        {
        }

        public RegisterRepository(BinaryReader reader, ExtendedActorSystem system)
            : base(reader, system)
        { }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            IgnoreDuplicate = reader.ReadBoolean();
            base.ReadInternal(reader, manifest, system);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(IgnoreDuplicate);
            base.WriteInternal(writer);
        }
    }
}