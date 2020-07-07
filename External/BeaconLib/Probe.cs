using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Serilog;

namespace BeaconLib
{
    /// <summary>
    /// Counterpart of the beacon, searches for beacons
    /// </summary>
    /// <remarks>
    /// The beacon list event will not be raised on your main thread!
    /// </remarks>
    [PublicAPI]
    public sealed class Probe : IDisposable
    {
        private static ILogger _log = Log.ForContext<Probe>();

        /// <summary>
        /// Remove beacons older than this
        /// </summary>
        private static readonly TimeSpan BeaconTimeout = new TimeSpan(0, 0, 0, 5); // seconds

        public event Action<IEnumerable<BeaconLocation>>? BeaconsUpdated;

        private readonly Task _thread;
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly UdpClient _udp = new UdpClient();
        private IEnumerable<BeaconLocation> _currentBeacons = Enumerable.Empty<BeaconLocation>();

        private bool _running = true;

        public Probe(string beaconType)
        {
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            BeaconType = beaconType;
            _thread = new Task(BackgroundLoop, TaskCreationOptions.LongRunning);

            _log.Information("Bind Probe To Port 0");
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            try 
            {
                _udp.AllowNatTraversal(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error switching on NAT traversal");
            }

            _udp.BeginReceive(ResponseReceived, null);
        }

        public void Start()
        {
            _log.Information("Starting Probe");
            _thread.Start();
        }

        private void ResponseReceived(IAsyncResult ar)
        {
            _log.Information("Incomming Reponse");
            var remote = new IPEndPoint(IPAddress.Any, 0);
            var bytes = _udp.EndReceive(ar, ref remote);

            var typeBytes = Beacon.Encode(BeaconType);
            if (Beacon.HasPrefix(bytes, typeBytes))
            {
                try
                {
                    _log.Information("Processing Response");
                    var portBytes = bytes.Skip(typeBytes.Length).Take(2).ToArray();
                    var port      = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(portBytes, 0));
                    var payload   = Beacon.Decode(bytes.Skip(typeBytes.Length + 2));
                    NewBeacon(new BeaconLocation(new IPEndPoint(remote.Address, port), payload, DateTime.Now));
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Error on Decode Recived Beacon");
                }
            }
            else
            {
                _log.Information("Incompatiple Data");
            }

            _udp.BeginReceive(ResponseReceived, null);
        }

        public string BeaconType { get; private set; }

        private void BackgroundLoop()
        {
            while (_running)
            {
                try
                {
                    BroadcastProbe();
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Error on Sending");
                }

                _waitHandle.WaitOne(2000);
                PruneBeacons();
            }
        }

        private void BroadcastProbe()
        {
            _log.Information("Sending Request");
            var probe = Beacon.Encode(BeaconType).ToArray();
            _udp.Send(probe, probe.Length, new IPEndPoint(IPAddress.Broadcast, Beacon.DiscoveryPort));
        }

        private void PruneBeacons()
        {
            _log.Information("Prune Beacons");
            var cutOff = DateTime.Now - BeaconTimeout;
            var oldBeacons = _currentBeacons.ToList();
            var newBeacons = oldBeacons.Where(_ => _.LastAdvertised >= cutOff).ToList();
            if (EnumsEqual(oldBeacons, newBeacons)) return;

            var u = BeaconsUpdated;
            u?.Invoke(newBeacons);
            _currentBeacons = newBeacons;
        }

        private void NewBeacon(BeaconLocation newBeacon)
        {
            _log.Information("Updating Beacons");
            var newBeacons = _currentBeacons
                .Where(_ => !_.Equals(newBeacon))
                .Concat(new [] { newBeacon })
                .OrderBy(_ => _.Data)
                .ThenBy(_ => _.Address, IpEndPointComparer.Instance)
                .ToList();
            var u = BeaconsUpdated;
            u?.Invoke(newBeacons);
            _currentBeacons = newBeacons;
        }

        private static bool EnumsEqual<T>(List<T> xs, List<T> ys) => xs.Zip(ys, (x, y) => x != null && x.Equals(y)).Count() == xs.Count();

        public void Stop()
        {
            _log.Information("Stopping Probe");
            _running = false;
            _waitHandle.Set();
            _thread.Wait();
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error On Dispose Probe");
            }
        }
    }
}
