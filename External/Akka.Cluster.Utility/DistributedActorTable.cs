using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public class DistributedActorTable<TKey> : ReceiveActor
    {
        private readonly string _name;
        private readonly IActorRef _clusterActorDiscovery;
        private readonly IIdGenerator<TKey> _idGenerator;
        private readonly ILoggingAdapter _log;
        private readonly bool _underTestEnvironment;

        private readonly Dictionary<TKey, IActorRef> _actorMap = new Dictionary<TKey, IActorRef>();

        private class Container
        {
            //public DateTime LinkTime;
            public Dictionary<TKey, IActorRef> ActorMap;
        }

        private readonly Dictionary<IActorRef, Container> _containerMap = new Dictionary<IActorRef, Container>();
        private List<IActorRef> _containerWorkQueue;
        private int _lastWorkNodeIndex = -1;

        private enum RequestType
        {
            Create,
            GetOrCreate,
            Get,
        }

        private class Creating
        {
            public object[] Arguments;
            public DateTime RequestTime;
            public List<Tuple<IActorRef, RequestType>> Requesters;
            public IActorRef WorkingContainer;
        }

        private readonly Dictionary<TKey, Creating> _creatingMap = new Dictionary<TKey, Creating>();
        private bool _queuedCreatingExists;
        private readonly TimeSpan _createTimeout = TimeSpan.FromSeconds(10);

        private bool _stopping;

        public DistributedActorTable(string name, ActorSystem system)
            : this(name, ClusterActorDiscovery.Get(system).Discovery)
        {

        }

        public DistributedActorTable(string name, IActorRef clusterActorDiscovery)
            : this(name, clusterActorDiscovery, typeof(IncrementalIntegerIdGenerator), Array.Empty<object>())
            
        {
            
        }

        public DistributedActorTable(string name, IActorRef clusterActorDiscovery,
                                     Type idGeneratorType, object[] idGeneratorInitializeArgs)
        {
            _name = name;
            _clusterActorDiscovery = clusterActorDiscovery;
            _log = Context.GetLogger();

            if (idGeneratorType != null)
            {
                try
                {
                    _idGenerator = (IIdGenerator<TKey>)Activator.CreateInstance(idGeneratorType);
                    _idGenerator.Initialize(idGeneratorInitializeArgs);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Exception in initializing ${idGeneratorType.FullName}");
                    _idGenerator = null;
                }
            }

            Receive<ClusterActorDiscoveryMessage.ActorUp>(Handle);
            Receive<ClusterActorDiscoveryMessage.ActorDown>(Handle);

            Receive<DistributedActorTableMessage<TKey>.Create>(Handle);
            Receive<DistributedActorTableMessage<TKey>.GetOrCreate>(Handle);
            Receive<DistributedActorTableMessage<TKey>.Get>(Handle);
            Receive<DistributedActorTableMessage<TKey>.GetIds>(Handle);
            Receive<DistributedActorTableMessage<TKey>.GracefulStop>(Handle);

            Receive<DistributedActorTableMessage<TKey>.Internal.CreateReply>(Handle);
            Receive<DistributedActorTableMessage<TKey>.Internal.Add>(Handle);
            Receive<DistributedActorTableMessage<TKey>.Internal.Remove>(Handle);

            Receive<CreateTimeoutMessage>(Handle);
        }

        public DistributedActorTable(string primingTest,
                                     string name, IActorRef clusterActorDiscovery,
                                     Type idGeneratorType, object[] idGeneratorInitializeArgs)
            : this(name, clusterActorDiscovery, idGeneratorType, idGeneratorInitializeArgs)
        {
            if (primingTest != "TEST")
                throw new ArgumentException(nameof(primingTest));

            _underTestEnvironment = true;

            // Test environment doesn't use cluster so we need to watch container actors by itself.
            Receive<Terminated>(Handle);
        }

        protected override void PreStart()
        {
            _log.Info($"DistributedActorTable({_name}) Start");

            _clusterActorDiscovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, _name));
            _clusterActorDiscovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(_name + "Container"));

            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Self, new CreateTimeoutMessage(), Self);
        }

        private void Handle(ClusterActorDiscoveryMessage.ActorUp m)
        {
            _log.Info($"Container.ActorUp (Actor={m.Actor.Path})");

            if (_stopping)
            {
                _log.Info($"Ignore ActorUp while stopping. (Actor={m.Actor.Path})");
                return;
            }

            if (_containerMap.ContainsKey(m.Actor))
            {
                _log.Error($"I already have that container. (Actor={m.Actor.Path})");
                return;
            }

            var container = new Container
            {
                //LinkTime = DateTime.UtcNow,
                ActorMap = new Dictionary<TKey, IActorRef>()
            };
            _containerMap.Add(m.Actor, container);
            RebuildContainerWorkQueue();

            if (_queuedCreatingExists)
            {
                foreach (var (key, creating) in _creatingMap.Where(item => item.Value.WorkingContainer == null))
                {
                    creating.WorkingContainer = m.Actor;
                    container.ActorMap.Add(key, null);
                    m.Actor.Tell(new DistributedActorTableMessage<TKey>.Internal.Create(key, creating.Arguments));
                }

                _queuedCreatingExists = false;
            }

            if (_underTestEnvironment)
                Context.Watch(m.Actor);
        }

        private void Handle(ClusterActorDiscoveryMessage.ActorDown m)
        {
            _log.Info($"Container.ActorDown (Actor={m.Actor.Path})");

            if (_containerMap.TryGetValue(m.Actor, out var container) == false)
            {
                _log.Error($"I don't have that container. (Actor={m.Actor.Path})");
                return;
            }

            _containerMap.Remove(m.Actor);
            RebuildContainerWorkQueue();

            // Remove all actors owned by this container

            foreach (var (key, actorRef) in container.ActorMap)
            {
                _actorMap.Remove(key);
                if (actorRef != null) continue;
                
                // cancel all pending creating requests

                if (!_creatingMap.TryGetValue(key, out var creating)) continue;
                
                _creatingMap.Remove(key);

                foreach (var r in creating.Requesters)
                    r.Item1.Tell(CreateReplyMessage(r.Item2, key, null, false));
            }

            // When stopping done, ingest poison pill

            if (_stopping && _containerMap.Count == 0) 
                Context.Stop(Self);
        }

        private void Handle(Terminated m)
        {
            // This function should be used under test environment only.

            Handle(new ClusterActorDiscoveryMessage.ActorDown(m.ActorRef, null));
        }

        private IActorRef DecideWorkingContainer()
        {
            // round-robind

            if (_containerWorkQueue == null || _containerWorkQueue.Count == 0)
                return null;

            var index = _lastWorkNodeIndex + 1;
            if (index >= _containerWorkQueue.Count)
                index = 0;
            _lastWorkNodeIndex = index;

            return _containerWorkQueue[index];
        }

        private void RebuildContainerWorkQueue() 
            => _containerWorkQueue = _containerMap.Keys.ToList();

        private void CreateActor(RequestType requestType, TKey id, object[] args)
        {
            var container = DecideWorkingContainer();

            // add actor with creating status

            _actorMap.Add(id, null);

            _creatingMap.Add(id, new Creating
            {
                Arguments = args,
                RequestTime = DateTime.UtcNow,
                Requesters = new List<Tuple<IActorRef, RequestType>> { Tuple.Create(Sender, requestType) },
                WorkingContainer = container,
            });

            // send "create actor" request to container or enqueue it to pending list

            if (container != null)
            {
                _containerMap[container].ActorMap.Add(id, null);
                container.Tell(new DistributedActorTableMessage<TKey>.Internal.Create(id, args));
            }
            else
                _queuedCreatingExists = true;
        }

        private object CreateReplyMessage(RequestType requestType, TKey id, IActorRef actor, bool created)
        {
            return requestType switch
            {
                RequestType.Create => new DistributedActorTableMessage<TKey>.CreateReply(id, actor),
                RequestType.GetOrCreate => new DistributedActorTableMessage<TKey>.GetOrCreateReply(id, actor, created),
                RequestType.Get => new DistributedActorTableMessage<TKey>.GetReply(id, actor),
                _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null)
            };
        }

        private void PutOnCreateWaitingList(RequestType requestType, TKey id, IActorRef requester)
        {
            var (_, creating) = _creatingMap.FirstOrDefault(i => i.Key.Equals(id));
            if (creating == null)
            {
                _log.Error($"Cannot find creatingMap. (Id=${id} RequestType={requestType})");
                Sender.Tell(CreateReplyMessage(requestType, id, null, false));
                return;
            }

            creating.Requesters.Add(Tuple.Create(requester, requestType));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Create m)
        {
            if (_stopping)
                return;

            // decide ID (if provided, use it, otherwise generate new one)

            TKey id;
            if (m.Id.Equals(default(TKey)) == false)
            {
                if (_actorMap.ContainsKey(m.Id))
                {
                    Sender.Tell(new DistributedActorTableMessage<TKey>.CreateReply(m.Id, null));
                    return;
                }
                id = m.Id;
            }
            else
            {
                if (_idGenerator == null)
                {
                    _log.Error("I don't have ID Generator.");
                    Sender.Tell(new DistributedActorTableMessage<TKey>.CreateReply(m.Id, null));
                    return;
                }

                id = _idGenerator.GenerateId();
                if (_actorMap.ContainsKey(id))
                {
                    _log.Error($"ID generated by generator is duplicated. ID={id}, Actor={_actorMap[id]}");
                    Sender.Tell(new DistributedActorTableMessage<TKey>.CreateReply(m.Id, null));
                    return;
                }
            }

            CreateActor(RequestType.Create, id, m.Args);
        }

        private void Handle(DistributedActorTableMessage<TKey>.GetOrCreate m)
        {
            if (_stopping)
                return;

            var id = m.Id;

            // try to get actor

            if (_actorMap.TryGetValue(id, out var actor))
            {
                if (actor != null)
                {
                    Sender.Tell(new DistributedActorTableMessage<TKey>.GetOrCreateReply(m.Id, actor, false));
                    return;
                }

                PutOnCreateWaitingList(RequestType.GetOrCreate, id, Sender);
                return;
            }

            CreateActor(RequestType.GetOrCreate, id, m.Args);
        }

        private void Handle(DistributedActorTableMessage<TKey>.Get m)
        {
            var id = m.Id;

            // try to get actor

            if (_actorMap.TryGetValue(id, out var actor))
            {
                if (actor != null)
                {
                    Sender.Tell(new DistributedActorTableMessage<TKey>.GetReply(m.Id, actor));
                    return;
                }

                PutOnCreateWaitingList(RequestType.GetOrCreate, id, Sender);
                return;
            }

            Sender.Tell(new DistributedActorTableMessage<TKey>.GetReply(m.Id, null));
        }

        private void Handle(DistributedActorTableMessage<TKey>.GetIds m) 
            => Sender.Tell(new DistributedActorTableMessage<TKey>.GetIdsReply(_actorMap.Keys.ToArray()));

        private void Handle(DistributedActorTableMessage<TKey>.GracefulStop m)
        {
            if (_stopping)
                return;

            _stopping = true;

            if (_containerMap.Count > 0)
            {
                foreach (var i in _containerMap) 
                    i.Key.Tell(new DistributedActorTableMessage<TKey>.Internal.GracefulStop(m.StopMessage));
            }
            else
                Context.Stop(Self);
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.CreateReply m)
        {
            var id = m.Id;

            if (_actorMap.TryGetValue(id, out var actor) == false)
            {
                // request was already expired so ask to remove it
                _log.Info($"I got CreateReply but might be timeouted. (Id={id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.Remove(id));
                return;
            }

            if (actor != null)
            {
                _log.Error($"I got CreateReply but already have an actor. (Id={id} Actor={actor} ArrivedActor={m.Actor})");
                return;
            }

            if (_creatingMap.TryGetValue(id, out var creating) == false)
            {
                _log.Error($"I got CreateReply but I don't have a creating. Id={id}");
                return;
            }

            // update created actor in map

            _creatingMap.Remove(id);
            _actorMap[id] = m.Actor;
            _containerMap[creating.WorkingContainer].ActorMap[id] = m.Actor;

            // send reply to requesters

            for (var i = 0; i < creating.Requesters.Count; i++)
            {
                var requester = creating.Requesters[i];
                requester.Item1.Tell(CreateReplyMessage(requester.Item2, id, m.Actor, i == 0));
            }
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.Add m)
        {
            if (m.Actor == null)
            {
                _log.Error($"Invalid null actor for adding. (Id={m.Id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.AddReply(m.Id, m.Actor, false));
                return;
            }

            if (_containerMap.TryGetValue(Sender, out var container) == false)
            {
                _log.Error($"Cannot find a container trying to add an actor. (Id={m.Id} Container={Sender})");
                return;
            }

            try
            {
                _actorMap.Add(m.Id, m.Actor);
                container.ActorMap.Add(m.Id, m.Actor);
            }
            catch (Exception)
            {
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.AddReply(m.Id, m.Actor, false));
            }

            Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.AddReply(m.Id, m.Actor, true));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.Remove m)
        {
            if (_actorMap.TryGetValue(m.Id, out var actor) == false)
                return;

            if (actor == null)
            {
                _log.Error($"Cannot remove an actor waiting for creating. (Id={m.Id})");
                return;
            }

            if (_containerMap.TryGetValue(Sender, out var container) == false)
            {
                _log.Error($"Cannot find a container trying to remove an actor. (Id={m.Id} Container={Sender})");
                return;
            }

            if (container.ActorMap.ContainsKey(m.Id) == false)
            {
                _log.Error($"Cannot remove an actor owned by another container. (Id={m.Id} Container={Sender})");
                return;
            }

            _actorMap.Remove(m.Id);
            container.ActorMap.Remove(m.Id);
        }

        internal class CreateTimeoutMessage
        {
            #pragma warning disable 649
            public TimeSpan? Timeout;
            #pragma warning restore 649
        }

        private void Handle(CreateTimeoutMessage m)
        {
            var threshold = DateTime.UtcNow - (m.Timeout ?? _createTimeout);

            var expiredItems = _creatingMap.Where(i => i.Value.RequestTime <= threshold).ToList();
            foreach (var (id, creating) in expiredItems)
            {
                var container = creating.WorkingContainer;

                _log.Info($"CreateTimeout Id={id} Container={container}");

                // remove pending item

                _creatingMap.Remove(id);
                _actorMap.Remove(id);
                _containerMap[container].ActorMap.Remove(id);

                // send reply to requester

                foreach (var r in creating.Requesters)
                    r.Item1.Tell(CreateReplyMessage(r.Item2, id, null, false));
            }
        }
    }
}
