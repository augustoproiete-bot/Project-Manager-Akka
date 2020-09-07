using System;
using System.Threading;
using ServiceManagerIpProbe.Phases;
using Servicemnager.Networking;
using Servicemnager.Networking.Server;

namespace ServiceManagerIpProbe
{
    public sealed class OperationContext : IHasTimeout, IDisposable
    {
        public CancellationTokenSource GlobalTimeout { get; } = new CancellationTokenSource(TimeSpan.FromMinutes(10));

        public ManualResetEventSlim PhaseLock { get; } = new ManualResetEventSlim();

        public DataClient DataClient { get; set; }

        public HostConfiguration Configuration { get; set; }

        public string TargetFile { get; set; }

        public Action DestroySelf { get; set; } = () => { };

        bool IHasTimeout.IsTimeedOut => GlobalTimeout.IsCancellationRequested;

        public bool ProcessAndWait(NetworkMessage msg, EventHandler<MessageFromServerEventArgs> handler)
        {
            DataClient.OnMessageReceived += handler;

            try
            {
                PhaseLock.Reset();

                DataClient.Send(msg);

                PhaseLock.Wait(GlobalTimeout.Token);
                return !GlobalTimeout.IsCancellationRequested;
            }
            finally
            {
                DataClient.OnMessageReceived -= handler;
            }
        }

        public void Dispose()
        {
            GlobalTimeout.Dispose();
            PhaseLock?.Dispose();
        }
    }
}