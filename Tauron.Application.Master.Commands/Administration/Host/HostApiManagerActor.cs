using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Cluster.Utility;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;
using static Tauron.Application.Master.Commands.Administration.Host.InternalHostMessages;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class HostApiManagerActor : ExposedReceiveActor//, IWithTimers
    {
        public static Props CreateProps() 
            => Props.Create(() => new HostApiSuperviser());

        private readonly Dictionary<ActorPath, HostEntry> _entries = new Dictionary<ActorPath, HostEntry>();

        //public ITimerScheduler Timers { get; set; } = null!;

        private HostApiManagerActor()
        {
            var subscribeAbility = new SubscribeAbility(this);
            subscribeAbility.NewSubscription += subscribe =>
            {
                foreach (var (path, hostEntry) in _entries) 
                    subscribe.Intrest.Tell(new HostEntryChanged(hostEntry.Name, path, false));
            };

            Flow<ActorDown>(b =>
                b.Action(ad =>
                {
                    if (_entries.Remove(ad.Actor.Path, out var entry))
                        subscribeAbility.Send(new HostEntryChanged(entry.Name, entry.Actor.Path, true));
                }));

            Flow<ActorUp>(b =>
                b.Func(au =>
                    {
                        _entries[au.Actor.Path] = new HostEntry(string.Empty, au.Actor);
                        subscribeAbility.Send(new HostEntryChanged(string.Empty, au.Actor.Path, false));
                        return new GetHostName();
                    }).ToRefFromMsg(au => au.Actor)
                   .Then<GetHostNameResult>(b1 => b1.Action(r =>
                    {
                        if (!_entries.TryGetValue(Sender.Path, out var entry)) return;

                        entry.Name = r.Name;
                        subscribeAbility.Send(new HostEntryChanged(entry.Name, entry.Actor.Path, false));
                    })));

            Receive<CommandBase>(c =>
            {
                var he = _entries.Values.FirstOrDefault(e => e.Name == c.Target);
                if (he == null)
                {
                    Sender.Tell(new OperationResponse(false));
                    return;
                }

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

        //public sealed class CacheTimeout
        //{

        //}

        //private sealed class NotifyChange
        //{
        //    public ActorPath Key { get; }

        //    public NotifyChange(ActorPath key) => Key = key;
        //}

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

        private sealed class HostApiSuperviser : UntypedActor
        {
            private readonly IActorRef _hostApi;

            public HostApiSuperviser() => _hostApi = Context.ActorOf(() => new HostApiManagerActor(), "Manager");

            protected override void OnReceive(object message) => _hostApi.Forward(message);

            protected override SupervisorStrategy SupervisorStrategy()
            {
                return new OneForOneStrategy(
                    Decider.From(Directive.Resume,
                        Directive.Stop.When<ActorInitializationException>(),
                        Directive.Stop.When<ActorKilledException>(),
                        Directive.Stop.When<DeathPactException>()));
            }
        }
    }
}