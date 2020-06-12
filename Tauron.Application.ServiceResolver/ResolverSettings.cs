using Akka.Configuration;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver
{
    [PublicAPI]
    public sealed class ResolverSettings
    {
        public ResolverSettings(Config config)
        {
            Config = config;

            IsGlobal = Config.GetBoolean("akka.serviceresolver.isglobal");

            //ResolverPath = Config.GetString("akka.ServiceResolver.ResolverPath");

            Name = Config.GetString("akka.serviceresolver.name");
        }

        public Config Config { get; }

        public bool IsGlobal { get; set; }

        //public string ResolverPath { get; set; }

        public string Name { get; set; }

    }
}