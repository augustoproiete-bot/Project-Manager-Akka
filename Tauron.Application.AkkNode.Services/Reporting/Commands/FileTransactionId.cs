using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.Commands
{
    [PublicAPI]
    public sealed class FileTransactionId : InternalSerializableBase
    {
        public string Id { get; private set; } = string.Empty;

        public FileTransactionId(string id) => Id = id;

        public FileTransactionId(BinaryReader reader)
            : base(reader)
        { }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest) 
            => Id = reader.ReadString();

        protected override void WriteInternal(ActorBinaryWriter writer) 
            => writer.Write(Id);
    }
}