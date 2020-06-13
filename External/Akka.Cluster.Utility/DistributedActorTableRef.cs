using System;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    // Intefaced actor-ref for DistributedActorTable<TKey>
    [PublicAPI]
    public sealed class DistributedActorTableRef<TKey>
    {
        public IActorRef Target { get; }
        public TimeSpan? Timeout { get; }

        public DistributedActorTableRef(IActorRef target, TimeSpan? timeout = null)
        {
            Target = target;
            Timeout = timeout;
        }

        public DistributedActorTableRef<TKey> WithTimeout(TimeSpan? timeout) 
            => new DistributedActorTableRef<TKey>(Target, timeout);

        public Task<DistributedActorTableMessage<TKey>.CreateReply> Create(object[] args) =>
            Target.Ask<DistributedActorTableMessage<TKey>.CreateReply>(
                new DistributedActorTableMessage<TKey>.Create(args), Timeout);

        public Task<DistributedActorTableMessage<TKey>.CreateReply> Create(TKey id, object[] args) =>
            Target.Ask<DistributedActorTableMessage<TKey>.CreateReply>(
                new DistributedActorTableMessage<TKey>.Create(id, args), Timeout);

        public Task<DistributedActorTableMessage<TKey>.GetOrCreateReply> GetOrCreate(TKey id, object[] args) =>
            Target.Ask<DistributedActorTableMessage<TKey>.GetOrCreateReply>(
                new DistributedActorTableMessage<TKey>.GetOrCreate(id, args), Timeout);

        public Task<DistributedActorTableMessage<TKey>.GetReply> Get(TKey id) =>
            Target.Ask<DistributedActorTableMessage<TKey>.GetReply>(
                new DistributedActorTableMessage<TKey>.Get(id), Timeout);

        public Task<DistributedActorTableMessage<TKey>.GetIdsReply> GetIds() =>
            Target.Ask<DistributedActorTableMessage<TKey>.GetIdsReply>(
                new DistributedActorTableMessage<TKey>.GetIds(), Timeout);

        public void GracefulStop(object stopMessage) => 
            Target.Tell(new DistributedActorTableMessage<TKey>.GracefulStop(stopMessage));
    }

    // Intefaced actor-ref for DistributedActorTableContainer<TKey>
    [PublicAPI]
    public class DistributedActorTableContainerRef<TKey>
    {
        public IActorRef Target { get; }
        public TimeSpan? Timeout { get; }

        public DistributedActorTableContainerRef(IActorRef target, TimeSpan? timeout = null)
        {
            Target = target;
            Timeout = timeout;
        }

        public DistributedActorTableContainerRef<TKey> WithTimeout(TimeSpan? timeout) 
            => new DistributedActorTableContainerRef<TKey>(Target, timeout);

        public Task<DistributedActorTableMessage<TKey>.AddReply> Add(TKey id, IActorRef actor) =>
            Target.Ask<DistributedActorTableMessage<TKey>.AddReply>(
                new DistributedActorTableMessage<TKey>.Add(id, actor), Timeout);

        public void Remove(TKey id) 
            => Target.Tell(new DistributedActorTableMessage<TKey>.Remove(id));
    }
}
