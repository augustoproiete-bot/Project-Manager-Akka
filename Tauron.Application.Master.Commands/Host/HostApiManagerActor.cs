using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;
using static Tauron.Application.Master.Commands.Host.InternalHostMessages;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class HostApiManagerActor : ExposedReceiveActor
    {
        private readonly Dictionary<ActorPath, HostEntry> _entries = new Dictionary<ActorPath, HostEntry>();

        public HostApiManagerActor()
        {
            var subscribeAbility = new SubscribeAbility(this);
            subscribeAbility.NewSubscription += subscribe =>
            {
                foreach (var (path, hostEntry) in _entries) 
                    subscribe.intrest.Tell(new HostEntryChanged(hostEntry.Name, path, false));
            };

            this.Flow<ActorDown>()
               .From.Func(ad => _entries.Remove(ad.Actor.Path) ? new NotifyChange(ad.Actor.Path) : null).ToSelf();

            this.Flow<ActorUp>()
                .From.Func(au =>
                {
                    _entries[au.Actor.Path] = new HostEntry(string.Empty, au.Actor);
                    Self.Tell(new NotifyChange(au.Actor.Path));
                    return new GetHostName();
                }).ToRefFromMsg(au => au.Actor)
                .AndRespondTo<GetHostNameResult>().Func(r =>
                {
                    if (!_entries.TryGetValue(Sender.Path, out var entry)) return null;

                    entry.Name = r.Name;
                    return new NotifyChange(Sender.Path);

                }).ToSelf()
                .Then.Action(nc =>
                    subscribeAbility.Send(
                        _entries.TryGetValue(nc!.Key, out var entry)
                            ? new HostEntryChanged(entry.Name, nc.Key, false)
                            : new HostEntryChanged(string.Empty, nc.Key, true)));

            Receive<CommandBase>(c =>
            {
                var he = _entries.Values.FirstOrDefault(e => e.Name == c.Target);
                if(he == null) return;

                he.Actor.Forward(c);
            });

            subscribeAbility.MakeReceive();
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery.Get(Context.System)
               .Discovery.Tell(new MonitorActor(HostApi.ApiKey));

            base.PreStart();
        }

        private sealed class NotifyChange
        {
            public ActorPath Key { get; }

            public NotifyChange(ActorPath key) => Key = key;
        }

        private sealed class HostEntry
        {
            public string Name { get; set; }

            public IActorRef Actor { get; }

            public HostEntry(string name, IActorRef actor)
            {
                Name = name;
                Actor = actor;
            }
        }
    }
}