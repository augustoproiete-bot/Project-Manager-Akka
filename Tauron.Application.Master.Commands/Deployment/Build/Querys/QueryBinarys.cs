using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    [PublicAPI]
    public sealed class QueryBinarys : DeploymentQueryBase<FileTransactionId>
    {
        public IActorRef DataManager { get; private set; } = ActorRefs.Nobody;

        public int AppVersion { get; private set; }

        public QueryBinarys([NotNull] string appName, IActorRef dataManager, int appVersion = -1) 
            : base(appName)
        {
            DataManager = dataManager;
            AppVersion = appVersion;
        }

        public QueryBinarys([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) 
            : base(reader, system)
        {
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            DataManager = BinaryHelper.ReadRef(reader, system);
            AppVersion = reader.ReadInt32();
            base.ReadInternal(reader, manifest, system);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteRef(writer, DataManager);
            writer.Write(AppVersion);
            base.WriteInternal(writer);
        }
    }
}