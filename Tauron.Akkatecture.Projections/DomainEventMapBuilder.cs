using System;
using System.Threading.Tasks;
using Akkatecture.Aggregates;
using Akkatecture.Core;
using JetBrains.Annotations;
using LiquidProjections;
using LiquidProjections.MapBuilding;

namespace Tauron.Akkatecture.Projections
{
    [PublicAPI]
    public sealed class DomainEventMapBuilder<TProjection, TAggregate, TIdentity> 
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        private readonly EventMapBuilder<TProjection, TIdentity, ProjectionContext> _builder;

        public DomainEventMapBuilder() 
            => _builder = new EventMapBuilder<TProjection, TIdentity, ProjectionContext>();

        public IEventMap<ProjectionContext> Build(ProjectorMap<TProjection, TIdentity, ProjectionContext> map) => _builder.Build(map);

        public IEventMap<ProjectionContext> Build(RepositoryProjectorMap<TProjection, TIdentity> map)
            => _builder.Build(map.ProjectorMap);

        public DomainEventMapBuilder<TProjection, TAggregate, TIdentity> Where(Func<object, ProjectionContext, Task<bool>> predicate)
        {
           _builder.Where(predicate);
           return this;
        }

        public DomainEventMapBuilder<TProjection, TAggregate, TIdentity> Map<TEvent>(Action<ICrudAction<IDomainEvent<TAggregate, TIdentity, TEvent>, TProjection, TIdentity, ProjectionContext>> builder) 
            where TEvent : class, IAggregateEvent<TAggregate, TIdentity>
        {
            builder(_builder.Map<IDomainEvent<TAggregate, TIdentity, TEvent>>());
            return this;
        }
    }
}