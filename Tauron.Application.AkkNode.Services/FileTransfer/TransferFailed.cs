using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public enum FailReason
    {
        Unkowen,
        DuplicateOperationId,
        CorruptState,
        Deny,
        StreamError,
        ComunicationError,
        Timeout,
        ToManyResends,
        ReadError,
        WriteError
    }

    public sealed class TransferFailed : TransferMessages.TransferCompled
    {
        public FailReason Reason { get; private set; } = FailReason.Unkowen;

        public string? Data { get; private set; }

        public TransferFailed(string operationId, FailReason reason, string? data) 
            : base(operationId)
        {
            Reason = reason;
            Data = data;
        }

        public TransferFailed(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
            {
                Reason = (FailReason) reader.ReadInt32();
                Data = BinaryHelper.ReadNull(reader, r => r.ReadString());
            }
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write((int)Reason);
            BinaryHelper.WriteNull(Data, writer, writer.Write);
        }
    }
}