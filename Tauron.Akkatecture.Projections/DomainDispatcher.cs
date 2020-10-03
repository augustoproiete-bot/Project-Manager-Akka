using System;
using System.Threading.Tasks;
using Akkatecture.Core;
using JetBrains.Annotations;
using LiquidProjections;
using LiquidProjections.Abstractions;

namespace Tauron.Akkatecture.Projections
{
    [PublicAPI]
    public class DomainDispatcher<TProjection, TIdentity>
        where TProjection : class, IProjectorData<TIdentity> 
        where TIdentity : IIdentity
    {
        public AggregateEventReader Reader { get; }
        public DomainProjector Projector { get; }
        public IProjectionRepository Repo { get; }

        protected internal readonly Dispatcher Dispatcher;

        public DomainDispatcher(AggregateEventReader reader, DomainProjector projector, IProjectionRepository repo)
        {
            Reader = reader;
            Projector = projector;
            Repo = repo;

            Dispatcher = new Dispatcher(reader.CreateSubscription)
                         {
                             ExceptionHandler = ExceptionHandler,
                             SuccessHandler = SuccessHandler
                         };
        }

        protected virtual Task SuccessHandler(SubscriptionInfo info)
        {
            return Task.CompletedTask;
        }

        protected virtual Task<ExceptionResolution> ExceptionHandler(Exception exception, int attempts, SubscriptionInfo info) 
            => !exception.IsCriticalApplicationException() && attempts < 3 ? Task.FromResult(ExceptionResolution.Retry) : Task.FromResult(ExceptionResolution.Abort);

        public IDisposable Subscribe<TAggregate>()
        {
            var options = new SubscriptionOptions {Id = "Type@" + typeof(TAggregate).AssemblyQualifiedName};

            return Dispatcher.Subscribe(Repo.GetLastCheckpoint<TProjection, TIdentity>(), (list, info) => Projector.Projector.Handle(list), options);
        }

        public IDisposable Subscribe(string tag)
        {
            var options = new SubscriptionOptions { Id = "Tag@" + tag };

            return Dispatcher.Subscribe(Repo.GetLastCheckpoint<TProjection, TIdentity>(), (list, info) => Projector.Projector.Handle(list), options);
        }
    }
}