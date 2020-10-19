using System;

namespace Tauron.Application.AkkNode.Services.FileTransfer
{
    public interface ITransferData : IDisposable
    {
        int Read(byte[] buffer, in int offset, in int count);
        void Write(byte[] buffer, in int offset, in int count);
    }
}