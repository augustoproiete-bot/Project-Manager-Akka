using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Schema;

namespace Servicemnager.Networking.Server
{
    public class NetworkMessage
    {
        private sealed class SimplePool
        {
            private readonly ConcurrentQueue<PoolArray> _arrays = new ConcurrentQueue<PoolArray>();

            public PoolArray Rent() => _arrays.TryDequeue(out var array) ? array : new PoolArray(this);

            public void Return(PoolArray array)
            {
                array.Data.Clear();
                _arrays.Enqueue(array);
            }
        }

        private sealed class PoolArray : IDisposable
        {
            private readonly SimplePool _pool;

            public PoolArray(SimplePool pool) => _pool = pool;

            public List<byte> Data { get; } = new List<byte>();

            public void Dispose() => _pool.Return(this);
        }

        private static readonly SimplePool DataPool = new SimplePool();

        public static readonly byte[] Head = Encoding.ASCII.GetBytes("HEAD");

        public static readonly byte[] End = Encoding.ASCII.GetBytes("ENDING");

        public static byte[] WriteMessage(NetworkMessage msg)
        {
            using var message = DataPool.Rent();
            var typeLenght = Encoding.UTF8.GetByteCount(msg.Type);
            var lenght = Head.Length + End.Length + msg.RealLength + typeLenght + 12;
                
            message.Data.AddRange(Head);
            message.Data.AddRange(BitConverter.GetBytes(lenght));

            message.Data.AddRange(BitConverter.GetBytes(typeLenght));
            message.Data.AddRange(Encoding.UTF8.GetBytes(msg.Type));

            if (msg.Lenght == -1)
            {
                message.Data.AddRange(BitConverter.GetBytes(msg.Data.Length));
                message.Data.AddRange(msg.Data);
            }
            else
            {
                message.Data.AddRange(BitConverter.GetBytes(msg.Lenght));
                message.Data.AddRange(msg.Data.Take(msg.Lenght));
            }

            message.Data.AddRange(End);

            return message.Data.ToArray();
        }

        public static bool HasHeader(byte[] buffer)
        {
            var pos = 0;
            return CheckPresence(buffer, Head, ref pos);
        }

        public static bool HasTail(byte[] buffer)
        {
            if (buffer.Length < End.Length)
                return false;

            var pos = buffer.Length - End.Length;
            return CheckPresence(buffer, End, ref pos);
        }

        public static NetworkMessage ReadMessage(byte[] buffer)
        {
            int bufferPos = 0;

            if(!CheckPresence(buffer, Head, ref bufferPos))
                throw new InvalidOperationException("Invalid Message Format");

            var fullLenght = ReadInt(buffer, ref bufferPos);
            //if (fullLenght != buffer.Length)
            //    throw new InvalidOperationException("Invalid message Lenght");

            var typeLenght = ReadInt(buffer, ref bufferPos);
            var type = Encoding.UTF8.GetString(buffer, bufferPos, typeLenght);
            bufferPos += typeLenght;

            var dataLenght = ReadInt(buffer, ref bufferPos);
            var data = buffer.Skip(bufferPos).Take(dataLenght).ToArray();
            bufferPos += dataLenght;

            if (!CheckPresence(buffer, End, ref bufferPos) || fullLenght != bufferPos)
                throw new InvalidOperationException("Invalid Message Format");

            return new NetworkMessage(type, data, -1);
        }

        public static NetworkMessage Create(string type, byte[] data, int lenght = -1) => new NetworkMessage(type, data, lenght);

        public static NetworkMessage Create(string type) => new NetworkMessage(type, Array.Empty<byte>(), -1);

        private static int ReadInt(byte[] buffer, ref int pos)
        {
            int int32 = BitConverter.ToInt32(buffer, pos);
            pos = pos + 4;

            return int32;
        }

        [DebuggerHidden]
        private static bool CheckPresence(IReadOnlyList<byte> buffer, IEnumerable<byte> target, ref int pos)
        {
            foreach (var ent in target)
            {
                if (buffer[pos] != ent)
                    return false;

                pos++;
            }

            return true;
        }

        public string Type { get; }

        public byte[] Data { get; }

        public int Lenght { get; }


        public int RealLength => Lenght == -1 ? Data.Length : Lenght;

        private NetworkMessage(string type, byte[] data, int lenght)
        {
            Type = type;
            Data = data;
            Lenght = lenght;
        }
    }
}