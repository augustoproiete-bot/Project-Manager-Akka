using System.IO;
using Akka.Actor;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    public abstract class DeploymentAction : InternalSerializableBase, IReporterMessage
    {
        public string AppName { get; private set; } = string.Empty;

        public IActorRef Listner { get; protected set; } = ActorRefs.Nobody;

        string IReporterMessage.Info => AppName;

        protected DeploymentAction(string appName, IActorRef listner)
        {
            AppName = appName;
            Listner = listner;
        }

        protected DeploymentAction(BinaryReader reader, ExtendedActorSystem system)
            : base(reader, system)
        {

        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            AppName = reader.ReadString();
            Listner = BinaryHelper.ReadRef(reader, system);

            base.ReadInternal(reader, manifest, system);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(AppName);
            BinaryHelper.WriteRef(writer, Listner);
            base.WriteInternal(writer);
        }
    }
}