using System;
using SimpleTcp;

namespace ServiceManagerIpProbe.Server
{
    public sealed class DataClient
    {
        private readonly SimpleTcpClient _client;

        public DataClient(string host, int port = 0) => _client = new SimpleTcpClient(host, port, false, null, null);

        public void Connect() => _client.Connect();

        public event EventHandler Connected;

        public event EventHandler Disconnected;
    }
}