using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization.Extension;

namespace Tauron.Localization
{
    [PublicAPI]
    public static class LocExtensions
    {
        public static void RegisterLocalization(this ActorSystem system)
        {
            system.RegisterExtension(new LocExtensionId());
        }

        public static LocExtensionAdaptor Loc(this ActorSystem system)
        {
            return new LocExtensionAdaptor(system.GetExtension<LocExtension>(), system);
        }

        public static LocExtensionAdaptor Loc(this IActorContext context)
        {
            return new LocExtensionAdaptor(context.System.GetExtension<LocExtension>(), context.System);
        }
    }
}