using JetBrains.Annotations;

namespace Akka.Code.Configuration.Extensions
{
    [PublicAPI]
    public static class Extensions
    {
        public static AkkaRootConfiguration AddHyperion(this AkkaRootConfiguration config)
        {
            var c = config.Akka.Actor;
            c.Serializers["hyperion"] = "Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion";
            c.SerializationBindings["System.Object"] = "hyperion";

            return config;
        }
    }
}