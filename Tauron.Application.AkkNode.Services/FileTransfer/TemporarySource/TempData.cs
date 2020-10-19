using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer.TemporarySource
{
    [PublicAPI]
    public sealed class TempData : ITransferData
    {
        private readonly Stream _stream;
        private readonly TempFile _file;

        public TempData(TempFile file)
        {
            file.AddUsage();
            _stream = file.GetStream();
            _file = file;
        }

        public void Dispose()
        {
            _file.RemoveUage();
            _file.TryDispose();
        }

        public int Read(byte[] buffer, in int offset, in int count) => _stream.Read(buffer, offset, count);

        public void Write(byte[] buffer, in int offset, in int count) => _stream.Write(buffer, offset, count);
    }
}