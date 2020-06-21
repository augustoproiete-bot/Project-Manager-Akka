using System.Collections;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public sealed class ServiceRegistry
    {
        private readonly IActorRef _target;

        private ServiceRegistry(IActorRef target) => _target = target;

        public void RegisterService(RegisterService service)
            => _target.Tell(service);

        public void QueryService(QueryRegistratedServices query)
            => _target.Tell(query);

        private static readonly object Lock = new object();
        private static ServiceRegistry? _registry;

        public static void Start(ActorSystem system, RegisterService? self) 
            => system.ActorOf(Props.Create(() => new ServiceRegistryServiceActor(ClusterActorDiscovery.Get(system).Discovery, self)), nameof(ServiceRegistry));

        public static ServiceRegistry GetRegistry(ActorSystem refFactory)
        {
            lock (Lock)
                return _registry ??= new ServiceRegistry(refFactory.ActorOf(Props.Create(() => new ServiceRegistryClientActor()), nameof(ServiceRegistry)));
        }

        private sealed class ServiceRegistryClientActor : ExposedReceiveActor
        {
            private class CircularBuffer<T> : IEnumerable<T>
            {
                private readonly  List<T> _data = new List<T>();
                private int _curr = -1;

                public void Add(T data)
                    => _data.Add(data);

                public void Remove(T data)
                    => _data.Remove(data);

                public T Next()
                {
                    if (_data.Count == 0)
                        return default!;

                    _curr++;
                    if (_curr >= _data.Count)
                        _curr = 0;
                    return _data[_curr];
                }

                public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _data).GetEnumerator();
            }

            private readonly CircularBuffer<IActorRef> _serviceRegistrys = new CircularBuffer<IActorRef>();
            private readonly IActorRef _discovery;

            public ServiceRegistryClientActor()
            {
                _discovery = ClusterActorDiscovery.Get(Context.System).Discovery;

                this.Flow<ClusterActorDiscoveryMessage.ActorUp>()
                   .From.Action(au => _serviceRegistrys.Add(au.Actor))
                   .AndReceive();
                this.Flow<ClusterActorDiscoveryMessage.ActorDown>()
                   .From.Action(ad => _serviceRegistrys.Remove(ad.Actor))
                   .AndReceive();

                Receive<RegisterService>(s => _serviceRegistrys.Foreach(r => r.Tell(s)));
                Receive<QueryRegistratedServices>(rs =>
                                                  {
                                                      var target =_serviceRegistrys.Next();
                                                      if(target == null)
                                                          rs.Sender.Tell(new QueryRegistratedServicesResponse(new Dictionary<UniqueAddress, string>()));
                                                      else
                                                          target.Tell(rs);
                                                  });
            }

            protected override void PreStart()
            {
                _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistry)));
                base.PreStart();
            }
        }

        private sealed class ServiceRegistryServiceActor : ExposedReceiveActor
        {
            private sealed class SyncRegistry
            {
                public Dictionary<UniqueAddress, string> ToSync { get; }

                public SyncRegistry(Dictionary<UniqueAddress, string> sync) => ToSync = sync;
            }

            private readonly IActorRef _discovery;
            private readonly Dictionary<UniqueAddress, string> _services = new Dictionary<UniqueAddress, string>();

            public ServiceRegistryServiceActor(IActorRef discovery, RegisterService? self)
            {
                _discovery = discovery;
                if (self != null) _services[self.Address] = self.Name;

                Receive<RegisterService>(service => _services[service.Address] = service.Name);
                this.Flow<QueryRegistratedServices>()
                   .From.Func(qrs => new QueryRegistratedServicesResponse(new Dictionary<UniqueAddress, string>(_services))).ToRefFromMsg(rs => rs.Sender)
                   .AndReceive();

                this.Flow<ClusterActorDiscoveryMessage.ActorUp>()
                   .From.Action(When<ClusterActorDiscoveryMessage.ActorUp>(au => !au.Actor.Equals(Self), au => au.Actor.Tell(new SyncRegistry(_services))))
                   .AndReceive();

                Receive<ClusterActorDiscoveryMessage.ActorDown>(_ => { });

                this.Flow<SyncRegistry>()
                   .From.Action(sr => sr.ToSync.Foreach(kp => _services[kp.Key] = kp.Value))
                   .AndReceive();
            }

            protected override void PreStart()
            {
                _discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(ServiceRegistry)));
                _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistry)));
                base.PreStart();
            }
        }
    }
}