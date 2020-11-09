using System.Collections.Immutable;
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

        private AppConfig Config { get; }

        public ImmutableList<string> Seeds => Config.SeedUrls;

        public string SelfAddress { get; }

        public ClusterConfiguration(AppConfig config, ActorSystem system)
        {
            Config = config;
            SelfAddress = Cluster.Get(system).SelfAddress.ToString();
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