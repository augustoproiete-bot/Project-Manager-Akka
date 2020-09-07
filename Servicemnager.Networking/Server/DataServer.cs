using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using SimpleTcp;

namespace Servicemnager.Networking.Server
{
    public sealed class MessageFromClientEventArgs : EventArgs
    {
        public NetworkMessage Message { get; }

        public string Client { get; set; }

        public MessageFromClientEventArgs(NetworkMessage message, string client)
        {
            Message = message;
            Client = client;
        }
    }

    public sealed class DataServer : IDisposable
    {
        private readonly SimpleTcpServer _server;

        private EndPoint _endPoint;

        public EndPoint EndPoint 
            => _endPoint ?? (_endPoint = ((TcpListener) _server.GetType().GetField("_Listener", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_server))?.LocalEndpoint);

        public DataServer(string host, int port = 0)
        {
            _server = new SimpleTcpServer(host, port, false, null, null);
            _server.Events.DataReceived += EventsOnDataReceived;
        }

        private void EventsOnDataReceived(object sender, DataReceivedFromClientEventArgs e)
        {
            OnMessageReceived?.Invoke(this, new MessageFromClientEventArgs(NetworkMessage.ReadMessage(e.Data), e.IpPort));
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected
        {
            add => _server.Events.ClientConnected += value;
            remove => _server.Events.ClientConnected -= value;
        }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected
        {
            add => _server.Events.ClientDisconnected += value;
            remove => _server.Events.ClientDisconnected -= value;
        }

        public event EventHandler<MessageFromClientEventArgs> OnMessageReceived;

        public void Start() => _server.Start();

        public void Send(string client, NetworkMessage message) => _server.Send(client, NetworkMessage.WriteMessage(message));

        public void Dispose() => _server?.Dispose();
    }
}