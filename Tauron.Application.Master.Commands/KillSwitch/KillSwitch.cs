using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public static class KillSwitch
    {
        private const string KillSwitchName = "KillSwitch";

        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(KillSwitch));

        private static IActorRef _switch = ActorRefs.Nobody;

        public sealed class KillWatcher : ExposedReceiveActor
        {

            public KillWatcher(KillRecpientType type)
            {
                Flow<ActorUp>(b =>
                    b.Func(au =>
                        {
                            Log.Info("Send ActorUp Back {Name}", au.Actor.Path);
                            return new ActorUp(Self, "None");
                        })
                       .ToRefFromMsg(au => au.Actor));

                Flow<RequestRegistration>(b =>
                    b.Func(rr =>
                    {
                        Log.Info("Sending Respond {Type}", type);
                        return new RespondRegistration(type);
                    }).ToSender());

                Flow<KillNode>(b =>
                    b.Action(kn =>
                    {
                        Log.Info("Leaving Cluster");
                        Cluster.Get(Context.System).LeaveAsync();
                    }));
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

                [UsedImplicitly]
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

                Flow<ActorDown>(b =>
                {
                    b.Action(ad =>
                    {
                        Log.Info("Remove Killswitch Actor {Name}", ad.Actor.Path);
                        _actors
                           .FindIndex(ae => ae.Target.Equals(ad.Actor))
                           .When(i => i != -1, i => _actors.RemoveAt(i));
                    });
                });

                Flow<ActorUp>(b =>
                {
                    b.Func(au =>
                        {
                            Log.Info("New killswitch Actor {Name}", au.Actor.Path);
                            return _actors
                               .AddAnd(ActorElement.New(au.Actor))
                               .To(_ => new RequestRegistration());
                        })
                       .ToRefFromMsg(au => au.Actor)
                       .Then<RespondRegistration>(b1 =>
                        {
                            b1.Action(r =>
                            {
                                Log.Info("Set Killswitch Actor Type {Type} {Name}", r.RecpientType, Context.Sender.Path);
                                _actors
                                   .Find(e => e.Target.Equals(Context.Sender))
                                   .When(i => i != null, element => element.RecpientType = r.RecpientType);
                            });
                        });
                });

                Flow<RequestRegistration>(b =>
                    b.Func(() => new RespondRegistration(KillRecpientType.Seed))
                       .ToSender());

                Flow<KillClusterMsg>(b => b.Action(RunKillCluster));

                Flow<KillNode>(b =>
                    b.Action(_ =>
                    {
                        Log.Info("Leaving Cluster");
                        Cluster.Get(Context.System).LeaveAsync();
                    }));

                Flow<ActorUp>(b =>
                    b.Action(au => au.When(u => u.Tag == "None", up =>
                        {
                            Log.Info("Incoming kill Watcher {Name}", up.Actor.Path);
                            Context.Watch(up.Actor);
                        }))
                       .Then<Terminated>(b1 => b1.Func(t => new ActorDown(t.ActorRef, "None")).ToSelf()));
            }

            private void RunKillCluster()
            {
                Log.Info("Begin Cluster Shutdown");

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
                    var actors = dic[recpientType];
                    Log.Info("Tell Shutdown {Type} -- {Count}", recpientType, actors.Count);
                    foreach (var actorRef in actors) 
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

        public static void Setup(ActorSystem system)
        {
            Log.Information("Setup Killswitch");
            _switch = system.ActorOf(() => new KillSwitchActor(system), KillSwitchName);
        }

        public static void Subscribe(ActorSystem system, KillRecpientType type)
        {
            Log.Information("SubscribeToEvent Killswitch");
            _switch = system.ActorOf(() => new KillWatcher(type), KillSwitchName);
        }

        public static void KillCluster()
            => _switch.Tell(new KillClusterMsg());
    }
}