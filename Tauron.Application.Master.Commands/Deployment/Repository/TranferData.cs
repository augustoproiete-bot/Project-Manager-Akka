using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed class Tranferdata : InternalSerializableBase
    {
        public FileTransactionId Id { get; private set; }

        public string Commit { get; private set; }

        public Tranferdata(FileTransactionId id, string commit)
        {
            Id = id;
            Commit = commit;
        }

        public Tranferdata(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            Id = new FileTransactionId(reader);
            Commit = reader.ReadString();
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            Id.Write(writer);
            writer.Write(Commit);
            base.WriteInternal(writer);
        }
    }
}