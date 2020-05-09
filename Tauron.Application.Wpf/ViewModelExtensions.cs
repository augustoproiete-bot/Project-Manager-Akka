using Akka.Actor;

namespace Tauron.Application.Wpf
{
    public static class ViewModelExtensions
    {
        public static void Tell(this IViewModel model, object msg)
            => model.Tell(msg, ActorRefs.NoSender);
    }
}