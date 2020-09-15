using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class OperationResult : InternalSerializableBase
    {
        public static OperationResult Success() => new OperationResult(true, null);

        public static OperationResult Failure(string error) => new OperationResult(false, error);

        public bool Ok { get; private set; }

        public string? Error { get; private set; }

        public OperationResult(BinaryReader reader)
            : base(reader)
        {
            
        }

        public OperationResult(bool ok, string? error)
        {
            Ok = ok;
            Error = error;
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Ok);
            BinaryHelper.WriteNull(Error, writer, writer.Write);

            base.WriteInternal(writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            Ok = reader.ReadBoolean();
            Error = BinaryHelper.ReadNull(reader, r => r.ReadString());

            base.ReadInternal(reader, manifest);
        }
    }
}