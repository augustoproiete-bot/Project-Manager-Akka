using System;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Cluster.Utility;
using Akka.DistributedData;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public static class KillSwitch
    {
        private const string KillSwitchName = "KillSwitch";

        private static IActorRef _nodes;

        private class InternalKillKey
        {
            public UniqueAddress Address { get; }

            public KillRecpientType RecpientType { get; }

            public InternalKillKey(UniqueAddress address, KillRecpientType recpientType)
            {
                Address = address;
                RecpientType = recpientType;
            }
        }

        public sealed class KillNode
        {
            
        }

        public static void Setup(ActorSystem system)
        {
            _nodes = system.ActorOf(() => new DistributedActorTable<UniqueAddress>(KillSwitchName, system), KillSwitchName);
        }

        public static void Subscripe(ActorSystem system, IActorRef recpient)
        {
        }



        //public static void KillCluster(ActorSystem system)
        //{
        //    var cluster = Cluster.Get(system);
        //    if (!cluster.SelfRoles.Contains("Master"))
        //        throw new InvalidOperationException("Only Masters able to Kill cluster");
        //    DistributedPubSub.Get(system).Mediator.Tell(new Publish(nameof(KillNode), new KillNode()));
        //}

        //public static void Subscripe(ActorSystem system, IActorRef recpient) 
        //    => DistributedPubSub.Get(system).Mediator.Tell(new Subscribe(nameof(KillNode), recpient));

        //public static void Enable(ActorSystem system)
        //{
        //    system.ActorOf((dsl, cxt) =>
        //                   {
        //                       dsl.Receive<SubscribeAck>((s, c) => { });
        //                       dsl.Receive<KillNode>((node, context) => Cluster.Get(context.System).LeaveAsync()); 

        //                       Subscripe(system, cxt.Self);
        //                   }, "Simple-Kill-Switch");
        //}
    }
}