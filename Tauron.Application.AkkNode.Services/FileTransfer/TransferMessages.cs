using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public static class TransferMessages
    {
        public abstract class TransferMessage
        {
            public string OperationId { get; }

            protected TransferMessage(string operationId) => OperationId = operationId;
        }

        public sealed class RequestAccept : TransferMessage
        {
            public Func<Stream> Target { get; }

            public RequestAccept(string operationId, Func<Stream> target)
                : base(operationId)
            {
                Target = target;
            }
        }

        public sealed class RequestDeny : TransferMessage
        {
            public RequestDeny(string operationId) : base(operationId)
            {
            }
        }
    }
}