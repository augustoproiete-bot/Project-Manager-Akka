using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public sealed class TransferCompled : TransferMessages.TransferCompled
    {
        public TransferCompled(string operationId, string? data) 
            : base(operationId, data)
        {
        }

        public TransferCompled(BinaryReader reader)
            : base(reader)
        {
            
        }
    }
}