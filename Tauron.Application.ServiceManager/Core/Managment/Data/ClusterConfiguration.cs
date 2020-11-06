using Akka.Actor;
using Akka.Cluster;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    public sealed class ClusterConfiguration : IStateEntity, ICanApplyChange<ClusterConfiguration>
    {
        bool IStateEntity.IsDeleted => false;
        string IStateEntity.Id => nameof(ClusterConfiguration);

        public AppConfig Config { get; }

        public Cluster Cluster { get; }

        public ClusterConfiguration(AppConfig config, ActorSystem system)
        {
            Config = config;
            Cluster = Cluster.Get(system);
        }

        public ClusterConfiguration Apply(MutatingChange apply)
        {
            switch (apply)
            {
                case AddSeedUrlEvent addSeed:
                    Config.SeedUrls = Config.SeedUrls.Add(addSeed.SeedUrl);
                    return this;
                case RemoveSeedUrlEvent removeSeed:
                    Config.SeedUrls = Config.SeedUrls.Remove(removeSeed.SeedUrl);
                    return this;
                default:
                    return this;
            }
        }
    }
}