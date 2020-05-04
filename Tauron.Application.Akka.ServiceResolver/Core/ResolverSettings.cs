using Akka.Actor;
using Akka.Configuration;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Core
{
    [PublicAPI]
    public sealed class ResolverSettings
    {
        public Config Config { get; }

        public bool IsGlobal { get; set; }

        public string ResolverPath { get; set; }

        public string Name { get; set; }


        public ResolverSettings(Config config)
        {
            Config = config;

            IsGlobal = Config.GetBoolean("akka.ServiceResolver.IsGlobal");

            ResolverPath = Config.GetString("akka.ServiceResolver.ResolverPath");

            Name = Config.GetString("akka.ServiceResolver.Name");
        }

        public bool Verify(ActorSystem system)
        {
            if (IsGlobal)
                return true;

            if (!string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(ResolverPath))
                return true;

            system.Terminate();
            return false;

        }
    }
}