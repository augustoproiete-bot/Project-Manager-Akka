using System;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Cluster;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.ServiceManager.Core.Managment.States;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.Core.Model
{
    [Processor]
    public class SeedProcessor : ActorModel
    {
        private static Cluster Cluster => Cluster.Get(Context.System);

        private readonly CommonAppInfo _appInfo;

        public SeedProcessor(IActionInvoker actionInvoker, CommonAppInfo appInfo, ActorSystem system) 
            : base(actionInvoker)
        {
            Cluster.Get(system).RegisterOnMemberUp(() => appInfo.ConnectionState = ConnectionState.Online);
            Cluster.Get(system).RegisterOnMemberRemoved(() => appInfo.ConnectionState = ConnectionState.Offline);

            _appInfo = appInfo;

            WhenStateChanges<SeedState>().FromEvent(s => s.TryJoin)
               .ToFlow().Action(TryJoin);

            WhenStateChanges<SeedState>().FromEvent(s => s.AddSeed)
               .ToFlow().Action(AddSeedUrl);

            WhenStateChanges<SeedState>().FromEvent(s => s.RemoveSeed)
               .ToFlow().Action(RemoveSeed);
        }

        private void RemoveSeed(RemoveSeedUrlEvent obj)
        {
            if (obj.RemaningCount == 0)
                Cluster.LeaveAsync();
        }

        private void AddSeedUrl(AddSeedUrlEvent evt)
        {
            Log.Info("Try Add Seed Url {Url}", evt.SeedUrl);

            Cluster.Join(Address.Parse(evt.SeedUrl));
        }

        private void TryJoin(TryJoinEvent evt)
        {
            _appInfo.ConnectionState = ConnectionState.Connecting;
            Log.Info("Trying Join to Cluster");
            if (Cluster.State.Members.Count != 0)
            {
                Log.Info("Cluster Joins is already Compled");
                return;
            }

            var source = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            Cluster.JoinSeedNodesAsync(evt.Addreses.Select(Address.Parse), source.Token);
        }
    }
}