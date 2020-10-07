using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands.Deployment.Deployment.Data;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Querys
{
    [PublicAPI]
    public sealed class QueryBinarys : DeploymentQueryBase<FileTransactionId>
    {
        public IActorRef DataManager { get; private set; } = ActorRefs.Nobody;

        public int Version { get; private set; }

        public QueryBinarys([NotNull] string appName, IActorRef dataManager, int version = -1) 
            : base(appName)
        {
            DataManager = dataManager;
            Version = version;
        }

        public QueryBinarys([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) 
            : base(reader, system)
        {
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            DataManager = BinaryHelper.ReadRef(reader, system);
            Version = reader.ReadInt32();
            base.ReadInternal(reader, manifest, system);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteRef(writer, DataManager);
            writer.Write(Version);
            base.WriteInternal(writer);
        }
    }
}