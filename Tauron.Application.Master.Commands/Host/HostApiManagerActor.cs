using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Tauron.Akka;
using static Tauron.Application.Master.Commands.Host.InternalHostMessages;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.Host
{
    public sealed class HostApiManagerActor : ExposedReceiveActor
    {
        private readonly Dictionary<ActorPath, HostEntry> _entries = new Dictionary<ActorPath, HostEntry>();

        public HostApiManagerActor()
        {
            this.Flow<ActorDown>()
               .From.Action(ad => _entries.Remove(ad.Actor.Path));

            this.Flow<ActorUp>()
               .From.Func(au =>
                {
                    _entries[au.Actor.Path] = new HostEntry(string.Empty, au.Actor);
                    return new GetHostName();
                }).ToRefFromMsg(au => au.Actor)
               .AndRespondTo<GetHostNameResult>()
               .Action(r =>
                {
                    if (_entries.TryGetValue(Sender.Path, out var entry))
                        entry.Name = r.Name;
                });

            Receive<CommandBase>(c =>
            {
                var he = _entries.Values.FirstOrDefault(e => e.Name == c.Target);
                if(he == null) return;

                he.Actor.Forward(c);
            });
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery.Get(Context.System)
               .Discovery.Tell(new MonitorActor(HostApi.ApiKey));

            base.PreStart();
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