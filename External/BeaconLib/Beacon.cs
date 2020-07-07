using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using Serilog;

namespace BeaconLib
{
    /// <summary>
    /// Instances of this class can be autodiscovered on the local network through UDP broadcasts
    /// </summary>
    /// <remarks>
    /// The advertisement consists of the beacon's application type and a short beacon-specific string.
    /// </remarks>
    [PublicAPI]
    public sealed class Beacon : IDisposable
    {
        private static ILogger _log = Log.ForContext<Beacon>();

        internal const int DiscoveryPort = 35891;
        private readonly UdpClient _udp;
 
        public Beacon(string beaconType, ushort advertisedPort)
        {
            BeaconType     = beaconType;
            AdvertisedPort = advertisedPort;
            BeaconData     = "";
            
            _log.Information("Bind UDP beacon to {Port}", DiscoveryPort);
            _udp = new UdpClient();
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));

            try 
            {
                _udp.AllowNatTraversal(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error switching on NAT traversal");
            }
        }

        public void Start()
        {
            _log.Information("Starting Beacon");
            Stopped = false;
            _udp.BeginReceive(ProbeReceived, null);
        }

        public void Stop() => Stopped = true;

        private void ProbeReceived(IAsyncResult ar)
        {
            var remote = new IPEndPoint(IPAddress.Any, 0);
            var bytes  = _udp.EndReceive(ar, ref remote);
            _log.Information("Incoming Probe {Adress}", remote);

            // Compare beacon type to probe type
            var typeBytes = Encode(BeaconType);
            if (HasPrefix(bytes, typeBytes))
            {
                _log.Information("Responding Probe {Adress}", remote);
                // If true, respond again with our type, port and payload
                var responseData = Encode(BeaconType)
                    .Concat(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)AdvertisedPort)))
                    .Concat(Encode(BeaconData)).ToArray();
                _udp.Send(responseData, responseData.Length, remote);
            }
            else
                _log.Information("Incompatible Data");

            if (!Stopped) _udp.BeginReceive(ProbeReceived, null);
        }

        internal static bool HasPrefix<T>(T[] haystack, T[] prefix)
        {
            return haystack.Length >= prefix.Length &&
                haystack.Zip(prefix, (a, b) => a != null && a.Equals(b)).All(_ => _);
        }

        /// <summary>
        /// Convert a string to network bytes
        /// </summary>
        internal static byte[] Encode(string data) 
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var len = IPAddress.HostToNetworkOrder((short)bytes.Length);

            return BitConverter.GetBytes(len).Concat(bytes).ToArray();
        }

        /// <summary>
        /// Convert network bytes to a string
        /// </summary>
        internal static string Decode(IEnumerable<byte> data)
        {
            var listData = data as IList<byte> ?? data.ToList();

            var len = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(listData.Take(2).ToArray(), 0));
            if (listData.Count < 2 + len) throw new ArgumentException("Too few bytes in packet");

            return Encoding.UTF8.GetString(listData.Skip(2).Take(len).ToArray());
        }

        /// <summary>
        /// Return the machine's hostname (usually nice to mention in the beacon text)
        /// </summary>
        public static string HostName => Dns.GetHostName();

        public string BeaconType { get; private set; }
        public ushort AdvertisedPort { get; private set; }
        public bool Stopped { get; private set; }

        public string BeaconData { get; set; }

        public void Dispose() => Stop();
    }
}
