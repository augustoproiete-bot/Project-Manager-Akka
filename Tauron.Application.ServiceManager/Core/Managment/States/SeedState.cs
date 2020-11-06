using JetBrains.Annotations;
using Tauron.Application.ServiceManager.Core.Managment.Data;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;

namespace Tauron.Application.ServiceManager.Core.Managment.States
{
    [State]
    public sealed class SeedState : StateBase<ClusterConfiguration>
    {
        public IEventSource<TryJoinEvent> TryJoin { get; }

        public IEventSource<AddSeedUrlEvent> AddSeed { get; }

        public IEventSource<RemoveSeedUrlEvent> RemoveSeed { get; }

        public SeedState(ExtendedMutatingEngine<MutatingContext<ClusterConfiguration>> engine) : base(engine)
        {
            TryJoin = engine.EventSource<ClusterConfiguration, TryJoinEvent>();
            AddSeed = engine.EventSource<ClusterConfiguration, AddSeedUrlEvent>();
            RemoveSeed = engine.EventSource<ClusterConfiguration, RemoveSeedUrlEvent>();
        }
    }
}