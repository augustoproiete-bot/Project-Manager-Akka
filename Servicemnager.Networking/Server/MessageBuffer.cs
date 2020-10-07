using System;
using System.Collections.Generic;
using System.Linq;

namespace Servicemnager.Networking.Server
{
    public sealed class MessageBuffer
    {
        private readonly List<byte[]> _incomming = new List<byte[]>();
        private byte[] _merge = new byte[short.MaxValue * 2];

        public NetworkMessage? AddBuffer(byte[] buffer)
        {
            if(_incomming.Count == 0 && !NetworkMessage.HasHeader(buffer))
                throw new InvalidOperationException("Incomming Message has no header");
            if (_incomming.Count != 0 && buffer.Length < NetworkMessage.End.Length)
            {
                _incomming.Add(buffer);

                byte[] temp = Merge();

                if(NetworkMessage.HasTail(temp))
                    return NetworkMessage.ReadMessage(temp);
                
                _incomming.Add(temp);
                return null;
            }

            if (_incomming.Count == 0 && NetworkMessage.HasTail(buffer))
            {
                _incomming.Add(buffer);
                var merge = Merge();

                return NetworkMessage.ReadMessage(merge);
            }

            if (NetworkMessage.HasTail(buffer))
            {
                _incomming.Add(buffer);
                return NetworkMessage.ReadMessage(Merge());
            }

            _incomming.Add(buffer);
            return null;
        }

        private byte[] Merge()
        {
            var minLenght = _incomming.Sum(a => a.Length);
            if(_merge.Length <= minLenght)
                Array.Resize(ref _merge, minLenght + short.MaxValue / 2);

            var start = 0;

            foreach (var bytese in _incomming)
            {
                bytese.CopyTo(_merge, start);
                start += bytese.Length;
            }

            _incomming.Clear();
            return _merge;
        }
    }
}