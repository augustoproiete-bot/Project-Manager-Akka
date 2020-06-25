using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Tauron.Akka;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public static class KillSwitch
    {
        private const string KillSwitchName = "KillSwitch";


        private static IActorRef _switch = ActorRefs.Nobody;

        public sealed class KillWatcher : ExposedReceiveActor
        {
            
            public KillWatcher(KillRecpientType type)
            {
                this.Flow<ActorUp>()
                    .From.Func(au => new ActorUp(Self, "None"))
                    .ToRefFromMsg(au => au.Actor)
                    .AndReceive();

                this.Flow<RequestRegistration>()
                    .From.Func(rr => new RespondRegistration(type)).ToSender()
                    .AndReceive();

                this.Flow<KillNode>()
                    .From.Action(kn => Cluster.Get(Context.System).LeaveAsync())
                    .AndReceive();
            }

            protected override void PreStart()
            {
                ClusterActorDiscovery.Get(Context.System).Discovery.Tell(new MonitorActor(KillSwitchName));
                base.PreStart();
            }
        }

        public sealed class KillSwitchActor : ExposedReceiveActor
        {
            private sealed class ActorElement
            {
                public KillRecpientType RecpientType { get; set; } = KillRecpientType.Unkowen;

                public IActorRef Target { get; }

                public ActorElement(IActorRef target) => Target = target;

                public static ActorElement New(IActorRef r) 
                    => new ActorElement(r);
            }

            private static readonly KillRecpientType[] Order = {KillRecpientType.Unkowen, KillRecpientType.Frontend, KillRecpientType.Host, KillRecpientType.Service, KillRecpientType.Seed};

            private readonly IActorRef _actorDiscovery;

            private readonly List<ActorElement> _actors = new List<ActorElement>();

            public KillSwitchActor(ActorSystem system)
            {
                _actorDiscovery = ClusterActorDiscovery.Get(system).Discovery;

                this.Flow<ActorDown>()
                    .From.Action(ad => 
                        _actors
                        .FindIndex(ae => ae.Target.Equals(ad.Actor))
                        .When(i => i != -1, i => _actors.RemoveAt(i)))
                    .AndReceive();

                this.Flow<ActorUp>()
                    .From.Func(au => 
                        _actors
                        .AddAnd(ActorElement.New(au.Actor))
                        .To(_ => new RequestRegistration()))
                    .ToRefFromMsg(au => au.Actor)
                    .AndRespondTo<RespondRegistration>().Action(r => 
                        _actors
                        .Find(e => e.Target.Equals(Context.Sender))
                        .When(i => i != null, element => element.RecpientType = r.RecpientType))
                    .AndReceive();

                this.Flow<RequestRegistration>()
                    .From.Func(() => new RespondRegistration(KillRecpientType.Seed))
                    .ToSender()
                    .AndReceive();

                this.Flow<KillClusterMsg>()
                    .From.Action(RunKillCluster)
                    .AndReceive();

                this.Flow<KillNode>()
                    .From.Action(_ => Cluster.Get(Context.System).LeaveAsync())
                    .AndReceive();

                this.Flow<ActorUp>()
                    .From.Action(au => au.When(u => u.Tag == "None", up => Context.Watch(up.Actor)))
                    .AndRespondTo<Terminated>().Func(t => new ActorDown(t.ActorRef, "None")).ToSelf()
                    .AndReceive();
            }

            private void RunKillCluster()
            {
                var dic = new GroupDictionary<KillRecpientType, IActorRef>
                {
                    KillRecpientType.Unkowen,
                    KillRecpientType.Frontend,
                    KillRecpientType.Host,
                    KillRecpientType.Seed,
                    KillRecpientType.Service
                };

                foreach (var element in _actors) 
                    dic.Add(element.RecpientType, element.Target);

                foreach (var recpientType in Order)
                {
                    foreach (var actorRef in dic[recpientType]) 
                        actorRef.Tell(new KillNode());
                }
            }

            protected override void PreStart()
            {
                _actorDiscovery.Tell(new RegisterActor(Self, KillSwitchName));
                _actorDiscovery.Tell(new MonitorActor(KillSwitchName));
                base.PreStart();
            }
        }

        public sealed class RespondRegistration
        {
            public KillRecpientType RecpientType { get; }

            public RespondRegistration(KillRecpientType recpientType)
            {
                RecpientType = recpientType;
            }
        }

        public sealed class RequestRegistration
        {
            
        }

        public sealed class KillNode
        {
            
        }

        public sealed class KillClusterMsg
        {
            
        }

        public static void Setup(ActorSystem system) => _switch = system.ActorOf(() => new KillSwitchActor(system), KillSwitchName);
        
        public static void Subscribe(ActorSystem system, KillRecpientType type) => _switch = system.ActorOf(() => new KillWatcher(type), KillSwitchName);

        public static void KillCluster()
            => _switch.Tell(new KillClusterMsg());
    }
}