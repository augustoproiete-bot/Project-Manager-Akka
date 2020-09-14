using System;
using SimpleTcp;

namespace Servicemnager.Networking.Server
{
    public class MessageFromServerEventArgs : EventArgs
    {
        public NetworkMessage Message { get; }

        public MessageFromServerEventArgs(NetworkMessage message) => Message = message;
    }

    public sealed class DataClient
    {
        private readonly SimpleTcpClient _client;
        private readonly MessageBuffer _messageBuffer = new MessageBuffer();

        public DataClient(string host, int port = 0)
        {
            _client = new SimpleTcpClient(host, port, false, null, null);
            _client.Events.DataReceived += (sender, args) =>
            {
                var msg = _messageBuffer.AddBuffer(args.Data);
                if(msg != null)
                    OnMessageReceived?.Invoke(this, new MessageFromServerEventArgs(msg));
            };
        }

        public void Connect() => _client.Connect();

        public event EventHandler? Connected
        {
            add => _client.Events.Connected += value;
            remove => _client.Events.Connected -= value;
        }

        public event EventHandler? Disconnected
        {
            add => _client.Events.Disconnected += value;
            remove => _client.Events.Disconnected -= value;
        }

        public event EventHandler<MessageFromServerEventArgs>? OnMessageReceived;

        public void Send(NetworkMessage msg) 
            => _client.Send(NetworkMessage.WriteMessage(msg));
    }
}