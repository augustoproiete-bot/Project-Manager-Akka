using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public sealed class ClusterActorDiscovery : IExtension
    {
        public IActorRef Discovery { get; }

        public ClusterActorDiscovery(ExtendedActorSystem system) => Discovery = system.ActorOf<ClusterActorDiscoveryActor>(nameof(ClusterActorDiscovery));

        public static ClusterActorDiscovery Get(ActorSystem system)
            => system.GetExtension<ClusterActorDiscovery>();
    }

    [PublicAPI]
    public sealed class ClusterActorDiscoveryId : ExtensionIdProvider<ClusterActorDiscovery>
    {
        public override ClusterActorDiscovery CreateExtension(ExtendedActorSystem system) => new ClusterActorDiscovery(system);
    }

    [PublicAPI]
    public class ClusterActorDiscoveryActor : ReceiveActor
    {
        private readonly Cluster _cluster;
        private readonly ILoggingAdapter _log;
        private readonly string _name;

        // Per cluster-node data

        private class NodeItem
        {
            public UniqueAddress ClusterAddress;
            public List<ActorItem> ActorItems;
        }

        private readonly Dictionary<IActorRef, NodeItem> _nodeMap = new Dictionary<IActorRef, NodeItem>();

        // Actors in cluster

        private class ActorItem
        {
            public IActorRef Actor;
            public string Tag;
        }

        private readonly List<ActorItem> _actorItems = new List<ActorItem>();

        // Monitor items registered in this discovery actor

        private class MonitorItem
        {
            public IActorRef Actor;
            public string Tag;
        }

        private readonly List<MonitorItem> _monitorItems = new List<MonitorItem>();

        // Watching actors

        private readonly Dictionary<IActorRef, int[]> _actorWatchCountMap = new Dictionary<IActorRef, int[]>();

        public ClusterActorDiscoveryActor()
        {
            _cluster = Cluster.Get(Context.System);
            _name = Self.Path.Name;
            _log = Context.GetLogger();

            Receive<ClusterEvent.MemberUp>(Handle);
            Receive<ClusterEvent.ReachableMember>(Handle);
            Receive<ClusterEvent.UnreachableMember>(Handle);
            Receive<ClusterEvent.MemberRemoved>(Handle);
            Receive<ClusterActorDiscoveryMessage.RegisterCluster>(Handle);
            Receive<ClusterActorDiscoveryMessage.ResyncCluster>(Handle);
            // Receive<ClusterActorDiscoveryMessage.UnregisterCluster>(m => Handle(m));
            Receive<ClusterActorDiscoveryMessage.ClusterActorUp>(Handle);
            Receive<ClusterActorDiscoveryMessage.ClusterActorDown>(Handle);
            Receive<ClusterActorDiscoveryMessage.RegisterActor>(Handle);
            Receive<ClusterActorDiscoveryMessage.UnregisterActor>(Handle);
            Receive<ClusterActorDiscoveryMessage.MonitorActor>(Handle);
            Receive<ClusterActorDiscoveryMessage.UnmonitorActor>(Handle);
            Receive<Terminated>(Handle);
        }

        protected override void PreStart() 
            => _cluster?.Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, 
                typeof(ClusterEvent.MemberUp), typeof(ClusterEvent.ReachableMember), typeof(ClusterEvent.UnreachableMember), typeof(ClusterEvent.MemberRemoved));

        protected override void PostStop() 
            => _cluster?.Unsubscribe(Self);

        private void Handle(ClusterEvent.MemberUp m)
        {
            if (_cluster == null) return;
            
            if (_cluster.SelfUniqueAddress == m.Member.UniqueAddress)
            {
                var roles = string.Join(", ", _cluster.SelfRoles);
                _log.Info($"Cluster.Up: {_cluster.SelfUniqueAddress} Role={roles}");
            }
            else
            {
                var remoteDiscoveryActor = Context.ActorSelection(m.Member.Address + "/user/" + _name);
                remoteDiscoveryActor.Tell(
                    new ClusterActorDiscoveryMessage.RegisterCluster(
                        _cluster.SelfUniqueAddress,
                        _actorItems.Select(a => new ClusterActorDiscoveryMessage.ClusterActorUp(a.Actor, a.Tag)).ToList()));
            }
        }

        private void Handle(ClusterEvent.ReachableMember m)
        {
            if (_cluster == null) return;
            
            if (_cluster.SelfUniqueAddress == m.Member.UniqueAddress)
            {
                var roles = string.Join(", ", _cluster.SelfRoles);
                _log.Info($"Cluster.RechableMe: {_cluster.SelfUniqueAddress} Role={roles}");
            }
            else
            {
                _log.Info($"Cluster.Rechable: {m.Member.Address} Role={string.Join(",", m.Member.Roles)}");

                var remoteDiscoveryActor = Context.ActorSelection(m.Member.Address + "/user/" + _name);
                remoteDiscoveryActor.Tell(
                    new ClusterActorDiscoveryMessage.ResyncCluster(
                        _cluster.SelfUniqueAddress,
                        _actorItems.Select(a => new ClusterActorDiscoveryMessage.ClusterActorUp(a.Actor, a.Tag)).ToList(),
                        true));
            }
        }

        private void Handle(ClusterEvent.UnreachableMember m)
        {
            _log.Info($"Cluster.Unreachable: {m.Member.Address} Role={string.Join(",", m.Member.Roles)}");

            var (key, _) = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.Member.UniqueAddress);
            if (key != null)
                RemoveNode(key);
        }

        private void Handle(ClusterEvent.MemberRemoved m)
        {
            _log.Info($"Cluster.MemberRemoved: {m.Member.Address} Role={string.Join(",", m.Member.Roles)}");

            var (key, _) = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.Member.UniqueAddress);
            if (key != null)
                RemoveNode(key);
        }

        private void Handle(ClusterActorDiscoveryMessage.RegisterCluster m)
        {
            _log.Info($"RegisterCluster: {m.ClusterAddress}");

            // Register node

            var item = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.ClusterAddress);
            if (item.Key != null)
            {
                _log.Error($"Already registered node. {m.ClusterAddress}");
                return;
            }

            _nodeMap.Add(Sender, new NodeItem
            {
                ClusterAddress = m.ClusterAddress,
                ActorItems = new List<ActorItem>()
            });

            // Process attached actorUp messages

            if (m.ActorUpList == null) return;
            
            foreach (var actorUp in m.ActorUpList)
                Handle(actorUp);
        }

        private void Handle(ClusterActorDiscoveryMessage.ResyncCluster m)
        {
            _log.Info($"ResyncCluster: {m.ClusterAddress} Request={m.Request}");

            // Reregister node

            var (key, _) = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.ClusterAddress);
            if (key != null)
                RemoveNode(key);

            _nodeMap.Add(Sender, new NodeItem
            {
                ClusterAddress = m.ClusterAddress,
                ActorItems = new List<ActorItem>()
            });

            // Process attached actorUp messages

            if (m.ActorUpList != null)
            {
                foreach (var actorUp in m.ActorUpList)
                    Handle(actorUp);
            }

            // Response

            if (m.Request)
            {
                Sender.Tell(
                    new ClusterActorDiscoveryMessage.ResyncCluster(
                        _cluster.SelfUniqueAddress,
                        _actorItems.Select(a => new ClusterActorDiscoveryMessage.ClusterActorUp(a.Actor, a.Tag)).ToList(),
                        false));
            }
        }

        //private void Handle(ClusterActorDiscoveryMessage.UnregisterCluster m)
        //{
        //    _log.Info($"UnregisterCluster: {m.ClusterAddress}");

        //    var item = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.ClusterAddress);
        //    if (item.Key != null)
        //        RemoveNode(item.Key);
        //}

        private void RemoveNode(IActorRef discoveryActor)
        {
            if (_nodeMap.TryGetValue(discoveryActor, out var node) == false)
                return;

            _nodeMap.Remove(discoveryActor);

            foreach (var actorItem in node.ActorItems)
                NotifyActorDownToMonitor(actorItem.Actor, actorItem.Tag);
        }

        private void Handle(ClusterActorDiscoveryMessage.ClusterActorUp m)
        {
            _log.Debug($"ClusterActorUp: Actor={m.Actor.Path} Tag={m.Tag}");

            if (_nodeMap.TryGetValue(Sender, out var node) == false)
            {
                _log.Error($"Cannot find node: Discovery={Sender.Path}");
                return;
            }

            node.ActorItems.Add(new ActorItem { Actor = m.Actor, Tag = m.Tag });

            NotifyActorUpToMonitor(m.Actor, m.Tag);
        }

        private void Handle(ClusterActorDiscoveryMessage.ClusterActorDown m)
        {
            _log.Debug($"ClusterActorDown: Actor={m.Actor.Path}");

            if (_nodeMap.TryGetValue(Sender, out var node) == false)
            {
                _log.Error($"Cannot find node: Discovery={Sender.Path}");
                return;
            }

            // remove actor from node.ActorItems

            var index = node.ActorItems.FindIndex(a => a.Actor.Equals(m.Actor));
            if (index == -1)
            {
                _log.Error($"Cannot find actor: Discovery={Sender.Path} Actor={m.Actor.Path}");
                return;
            }

            var tag = node.ActorItems[index].Tag;
            node.ActorItems.RemoveAt(index);

            NotifyActorDownToMonitor(m.Actor, tag);
        }

        private void Handle(ClusterActorDiscoveryMessage.RegisterActor m)
        {
            _log.Debug($"RegisterActor: Actor={m.Actor.Path} Tag={m.Tag}");

            // add actor to _actorItems

            var index = _actorItems.FindIndex(a => a.Actor.Equals(m.Actor));
            if (index != -1)
            {
                _log.Error($"Already registered actor: Actor={m.Actor.Path} Tag={m.Tag}");
                return;
            }

            _actorItems.Add(new ActorItem { Actor = m.Actor, Tag = m.Tag });
            WatchActor(m.Actor, 0);

            // tell monitors & other discovery actors that local actor up

            NotifyActorUpToMonitor(m.Actor, m.Tag);
            foreach (var discoveryActor in _nodeMap.Keys)
                discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorUp(m.Actor, m.Tag));
        }

        private void Handle(ClusterActorDiscoveryMessage.UnregisterActor m)
        {
            _log.Debug($"UnregisterActor: Actor={m.Actor.Path}");

            // remove actor from _actorItems

            var index = _actorItems.FindIndex(a => a.Actor.Equals(m.Actor));
            if (index == -1)
                return;

            var tag = _actorItems[index].Tag;
            _actorItems.RemoveAt(index);
            UnwatchActor(m.Actor, 0);

            // tell monitors & other discovery actors that local actor down

            NotifyActorDownToMonitor(m.Actor, tag);
            foreach (var discoveryActor in _nodeMap.Keys)
                discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorDown(m.Actor));
        }

        private void Handle(ClusterActorDiscoveryMessage.MonitorActor m)
        {
            _log.Debug($"MonitorActor: Monitor={Sender.Path} Tag={m.Tag}");

            _monitorItems.Add(new MonitorItem { Actor = Sender, Tag = m.Tag });
            WatchActor(Sender, 1);

            // Send actor up message to just registered monitor

            foreach (var actor in _actorItems.Where(a => a.Tag == m.Tag))
                Sender.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor.Actor, actor.Tag));

            foreach (var actor in _nodeMap.Values.SelectMany(node => node.ActorItems.Where(a => a.Tag == m.Tag))) 
                Sender.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor.Actor, actor.Tag));
        }

        private void Handle(ClusterActorDiscoveryMessage.UnmonitorActor m)
        {
            _log.Debug($"UnmonitorActor: Monitor={Sender.Path} Tag={m.Tag}");

            var count = _monitorItems.RemoveAll(w => w.Actor.Equals(Sender) && w.Tag == m.Tag);
            for (var i = 0; i < count; i++)
                UnwatchActor(Sender, 1);
        }

        private void Handle(Terminated m)
        {
            _log.Debug($"Terminated: Actor={m.ActorRef.Path}");

            if (_actorWatchCountMap.TryGetValue(m.ActorRef, out var counts) == false)
                return;

            if (counts[1] > 0)
            {
                _monitorItems.RemoveAll(w => w.Actor.Equals(Sender));
                counts[1] = 0;
            }

            if (counts[0] <= 0) return;
            
            var index = _actorItems.FindIndex(a => a.Actor.Equals(m.ActorRef));
            if (index == -1) return;
            
            var tag = _actorItems[index].Tag;
            _actorItems.RemoveAt(index);

            // tell monitors & other discovery actors that local actor down

            NotifyActorDownToMonitor(m.ActorRef, tag);
            foreach (var discoveryActor in _nodeMap.Keys)
                discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorDown(m.ActorRef));
        }

        private void NotifyActorUpToMonitor(IActorRef actor, string tag)
        {
            foreach (var monitor in _monitorItems.Where(w => w.Tag == tag))
                monitor.Actor.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor, tag));
        }

        private void NotifyActorDownToMonitor(IActorRef actor, string tag)
        {
            foreach (var monitor in _monitorItems.Where(w => w.Tag == tag))
                monitor.Actor.Tell(new ClusterActorDiscoveryMessage.ActorDown(actor, tag));
        }

        private void WatchActor(IActorRef actor, int channel)
        {
            // every watched actor counter has 2 values identified by channel
            // - channel 0: source actor watching counter
            // - channel 1: monitor actor watching counter (to track monitoring actor destroyed)

            if (_actorWatchCountMap.TryGetValue(actor, out var counts))
            {
                counts[channel] += 1;
                return;
            }

            counts = new int[2];
            counts[channel] += 1;
            _actorWatchCountMap.Add(actor, counts);
            Context.Watch(actor);
        }

        private void UnwatchActor(IActorRef actor, int channel)
        {
            if (_actorWatchCountMap.TryGetValue(actor, out var counts) == false)
                return;

            counts[channel] -= 1;
            if (counts.Sum() > 0)
                return;

            _actorWatchCountMap.Remove(actor);
            Context.Unwatch(actor);
        }
    }
}
