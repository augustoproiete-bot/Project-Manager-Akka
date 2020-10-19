using System.IO;

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
        public FailReason Reason { get; }

        public TransferFailed(string operationId, FailReason reason, string? data) 
            : base(operationId, data)
        {
            Reason = reason;
        }
    }
}