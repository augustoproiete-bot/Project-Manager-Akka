using System;
using System.IO;
using Servicemnager.Networking.Server;

namespace Servicemnager.Networking.Transmitter
{
    public sealed class Reciever : IDisposable
    {
        private readonly Func<Stream> _target;
        private readonly DataClient _client;

        private Stream? _stream;

        public Reciever(Func<Stream> target, DataClient client)
        {
            _target = target;
            _client = client;
        }

        public bool ProcessMessage(NetworkMessage msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case NetworkOperation.Deny:
                        throw new InvalidOperationException("Operation Cancelled from Server");
                    case NetworkOperation.DataAccept:
                        _stream = _target();
                        _client.Send(NetworkMessage.Create(NetworkOperation.DataNext));
                        return true;
                    case NetworkOperation.DataCompled:
                        return false;
                    case NetworkOperation.DataChunk:
                        if(_stream == null)
                            throw new InvalidOperationException("Write Stream is null");
                        _stream.Write(msg.Data, 0, msg.RealLength);
                        _client.Send(NetworkMessage.Create(NetworkOperation.DataNext));
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                _client.Send(NetworkMessage.Create(NetworkOperation.Deny));
                throw;
            }
        }

        public void Dispose() => _stream?.Dispose();
    }
}