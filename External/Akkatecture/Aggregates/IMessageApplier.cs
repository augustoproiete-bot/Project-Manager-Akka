using Akkatecture.Aggregates.Snapshot;
using Akkatecture.Core;
using JetBrains.Annotations;

namespace Akkatecture.Aggregates
{
    [PublicAPI]
    public interface IMessageApplier<TAggregate, TIdentity> : IEventApplier<TAggregate, TIdentity>, ISnapshotHydrater<TAggregate, TIdentity>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
    }
}