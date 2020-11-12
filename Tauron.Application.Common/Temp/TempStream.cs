using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public sealed class TempStream : Stream
    {
        private readonly TempFile _file;
        private readonly Stream _wrappedStream;

        public TempStream(TempFile file)
        {
            _file = file;
            _wrappedStream = file.InternalStrem;
        }

        /// <inheritdoc />
        public override bool CanRead => _wrappedStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _wrappedStream.CanSeek;

        /// <inheritdoc />
        public override bool CanTimeout => _wrappedStream.CanTimeout;

        /// <inheritdoc />
        public override bool CanWrite => _wrappedStream.CanWrite;

        /// <inheritdoc />
        public override long Length => _wrappedStream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _wrappedStream.Position;
            set => _wrappedStream.Position = value;
        }

        /// <inheritdoc />
        public override int ReadTimeout
        {
            get => _wrappedStream.ReadTimeout;
            set => _wrappedStream.ReadTimeout = value;
        }

        /// <inheritdoc />
        public override int WriteTimeout
        {
            get => _wrappedStream.WriteTimeout;
            set => _wrappedStream.WriteTimeout = value;
        }

        /// <inheritdoc />
        public override Task CopyToAsync(
          Stream destination,
          int bufferSize,
          CancellationToken cancellationToken)
        {
            return _wrappedStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(_file.NoStreamDispose) return;
            _file.Dispose();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => _wrappedStream.Equals(obj);

        /// <inheritdoc />
        public override void Flush() => _wrappedStream.Flush();

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken) => _wrappedStream.FlushAsync(cancellationToken);

        /// <inheritdoc />
        public override int GetHashCode() => _wrappedStream.GetHashCode();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _wrappedStream.Read(buffer, offset, count);

        /// <inheritdoc />
        public override Task<int> ReadAsync(
          byte[] buffer,
          int offset,
          int count,
          CancellationToken cancellationToken)
        {
            return _wrappedStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override int ReadByte() => _wrappedStream.ReadByte();

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _wrappedStream.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => _wrappedStream.SetLength(value);

        /// <inheritdoc />
        public override string? ToString() => _wrappedStream.ToString();

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _wrappedStream.Write(buffer, offset, count);

        /// <inheritdoc />
        public override Task WriteAsync(
          byte[] buffer,
          int offset,
          int count,
          CancellationToken cancellationToken)
        {
            return _wrappedStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override void WriteByte(byte value) => _wrappedStream.WriteByte(value);
    }
}