using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SimpleTcp;

namespace ServiceManagerIpProbe.Server
{
    public class NetworkMessage
    {
        private sealed class SimplePool
        {
            private ConcurrentQueue<PoolArray> _arrays = new ConcurrentQueue<PoolArray>();

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

        private static readonly byte[] Head = Encoding.ASCII.GetBytes("HEAD");

        private static readonly byte[] End = Encoding.ASCII.GetBytes("ENDING");
        
        public static void SendMessage(NetworkMessage msg, Action<byte[]> sender)
        {
            using (var message = DataPool.Rent())
            {
                int typeLenght = Encoding.UTF8.GetByteCount(msg.Type);
                int lenght = Head.Length + End.Length + msg.Data.Length + typeLenght  + 12;
                
                message.Data.AddRange(Head);
                message.Data.AddRange(BitConverter.GetBytes(lenght));

                message.Data.AddRange(BitConverter.GetBytes(typeLenght));
                message.Data.AddRange(Encoding.UTF8.GetBytes(msg.Type));

                message.Data.AddRange(BitConverter.GetBytes(msg.Data.Length));
                message.Data.AddRange(msg.Data);

                message.Data.AddRange(End);

                sender(message.Data.ToArray());
            }
        }

        public static NetworkMessage ReadMessage(byte[] buffer)
        {
            int bufferPos = 0;

            if(!CheckPresence(buffer, Head, ref bufferPos))
                throw new InvalidOperationException("Invalid Message Format");

            var fullLenght = ReadInt(buffer, ref bufferPos);
            if (fullLenght != buffer.Length)
                throw new InvalidOperationException("Invalid message Lenght");

            var typeLenght = ReadInt(buffer, ref bufferPos);
            var type = Encoding.UTF8.GetString(buffer, bufferPos, typeLenght);
            bufferPos += typeLenght;

            var dataLenght = ReadInt(buffer, ref bufferPos);
            var data = buffer.Skip(bufferPos).Take(dataLenght).ToArray();
            bufferPos += dataLenght;

            if (!CheckPresence(buffer, End, ref bufferPos) || buffer.Length != bufferPos)
                throw new InvalidOperationException("Invalid Message Format");

            return new NetworkMessage(type, data);
        }

        private static int ReadInt(byte[] buffer, ref int pos)
        {
            int int32 = BitConverter.ToInt32(buffer, pos);
            pos = pos + 4;

            return int32;
        }

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

        public NetworkMessage(string type, byte[] data)
        {
            Type = type;
            Data = data;
        }
    }
}