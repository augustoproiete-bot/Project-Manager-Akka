using System;
using System.IO;
using System.Threading;
using Servicemnager.Networking.Server;

namespace Servicemnager.Networking.Transmitter
{
    public sealed class Sender : IDisposable
    {
        private readonly Stream _toSend;
        private readonly string _client;
        private readonly DataServer _server;
        private readonly Func<byte[]> _getArray;
        private readonly Action<byte[]> _returnArray;
        private readonly Action<Exception> _errorHandler;

        private bool _isRunnging;

        public Sender(Stream toSend, string client, DataServer server, Func<byte[]> getArray, Action<byte[]> returnArray, Action<Exception> errorHandler)
        {
            _toSend = toSend;
            _client = client;
            _server = server;
            _getArray = getArray;
            _returnArray = returnArray;
            _errorHandler = errorHandler;
        }

        public bool ProcessMessage(NetworkMessage msg)
        {
            try
            {
                if (!_isRunnging)
                {
                    _server.Send(_client, NetworkMessage.Create(NetworkOperation.DataAccept));
                    _isRunnging = true;
                    return true;
                }

                switch (msg.Type)
                {
                    case NetworkOperation.Deny:
                        _isRunnging = false;
                        _toSend.Dispose();
                        _errorHandler(new InvalidOperationException("Operation Cancelled from Client"));
                        return false;
                    case NetworkOperation.DataNext:
                        var chunk = _getArray();
                        try
                        {
                            var count = _toSend.Read(chunk, 0, chunk.Length);
                            if (count == 0)
                            {
                                _toSend.Dispose();
                                _server.Send(_client, NetworkMessage.Create(NetworkOperation.DataCompled));
                                Thread.Sleep(2000);
                                return false;
                            }
                            _server.Send(_client, NetworkMessage.Create(NetworkOperation.DataChunk, chunk, count));
                            return true;
                        }
                        finally
                        {
                            _returnArray(chunk);
                        }
                    default:
                        _toSend.Dispose();
                        return false;
                }
            }
            catch (Exception e)
            {
                _toSend.Dispose();
                _errorHandler(e);
                _server.Send(_client, NetworkMessage.Create(NetworkOperation.Deny));
                return false;
            }
        }

        public void Dispose() => _toSend?.Dispose();
    }
}