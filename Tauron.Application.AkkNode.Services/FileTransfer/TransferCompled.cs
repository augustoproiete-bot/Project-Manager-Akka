using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public sealed class TransferCompled : TransferMessages.TransferCompled
    {
        public TransferCompled(string operationId) 
            : base(operationId)
        {
        }

        public TransferCompled(BinaryReader reader)
            : base(reader)
        {
            
        }
    }
}