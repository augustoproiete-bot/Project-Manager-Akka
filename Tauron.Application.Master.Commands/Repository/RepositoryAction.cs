using System.IO;
using Akka.Actor;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Repository
{
    public abstract class RepositoryAction : InternalSerializableBase
    {
        public string RepoName { get; private set; } = string.Empty;

        public IActorRef Listner { get; private set; } = ActorRefs.Nobody;

        protected RepositoryAction(string repoName, IActorRef listner)
        {
            RepoName = repoName;
            Listner = listner;
        }

        protected RepositoryAction(BinaryReader reader, ExtendedActorSystem system)
            : base(reader, system)
        {

        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            RepoName = reader.ReadString();
            Listner = BinaryHelper.ReadRef(reader, system);

            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(RepoName);
            BinaryHelper.WriteRef(writer, Listner);
            base.WriteInternal(writer);
        }
    }
}