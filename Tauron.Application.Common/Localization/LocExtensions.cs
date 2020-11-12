using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization.Extension;

namespace Tauron.Localization
{
    [PublicAPI]
    public static class LocExtensions
    {
        public static void RegisterLocalization(this ActorSystem system) 
            => system.RegisterExtension(new LocExtensionId());

        public static LocExtensionAdaptor Loc(this ActorSystem system) 
            => new(system.GetExtension<LocExtension>(), system);

        public static LocExtensionAdaptor Loc(this IActorContext context) 
            => new(context.System.GetExtension<LocExtension>(), context.System);
    }
}