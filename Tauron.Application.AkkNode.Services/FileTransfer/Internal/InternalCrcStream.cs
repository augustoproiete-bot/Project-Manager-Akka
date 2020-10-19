using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer.Internal
{
    /// <summary>
    ///     Encapsulates a <see cref="System.IO.Stream" /> to calculate the CRC32 checksum on-the-fly as data passes through.
    /// </summary>
    [PublicAPI]
    public class InternalCrcStream : IDisposable
    {
        private static uint[] _table = GenerateTable();

        private uint _readCrc = 0xFFFFFFFF;

        private uint _writeCrc = 0xFFFFFFFF;

        /// <summary>
        ///     Encapsulate a <see cref="System.IO.Stream" />.
        /// </summary>
        /// <param name="stream">The stream to calculate the checksum for.</param>
        public InternalCrcStream(ITransferData stream)
            => Stream = stream;

        /// <summary>
        ///     Gets the underlying stream.
        /// </summary>
        public ITransferData Stream { get; }

        /// <summary>
        ///     Gets the CRC checksum of the data that was read by the stream thus far.
        /// </summary>
        public uint ReadCrc => _readCrc ^ 0xFFFFFFFF;

        /// <summary>
        ///     Gets the CRC checksum of the data that was written to the stream thus far.
        /// </summary>
        public uint WriteCrc => _writeCrc ^ 0xFFFFFFFF;

        public int Read(byte[] buffer, int offset, int count)
        {
            count = Stream.Read(buffer, offset, count);
            _readCrc = CalculateCrc(_readCrc, buffer, offset, count);
            return count;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);

            _writeCrc = CalculateCrc(_writeCrc, buffer, offset, count);
        }

        [DebuggerStepThrough]
        private uint CalculateCrc(uint crc, byte[] buffer, int offset, int count)
        {
            unchecked
            {
                for (int i = offset, end = offset + count; i < end; i++)
                    crc = (crc >> 8) ^ _table[(crc ^ buffer[i]) & 0xFF];
            }

            return crc;
        }

        private static uint[] GenerateTable()
        {
            unchecked
            {
                uint[] table = new uint[256];

                const uint poly = 0xEDB88320;
                for (uint i = 0; i < table.Length; i++)
                {
                    var crc = i;
                    for (var j = 8; j > 0; j--)
                    {
                        if ((crc & 1) == 1)
                            crc = (crc >> 1) ^ poly;
                        else
                            crc >>= 1;
                    }

                    table[i] = crc;
                }

                return table;
            }
        }
        
        public void Dispose() => Stream.Dispose();
    }
}