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

        public TransferFailed(string operationId, FailReason reason, string? data) 
            : base(operationId, data)
        {
            Reason = reason;
        }

        public TransferFailed(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1)) 
                Reason = (FailReason) reader.ReadInt32();
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write((int)Reason);
        }
    }
}