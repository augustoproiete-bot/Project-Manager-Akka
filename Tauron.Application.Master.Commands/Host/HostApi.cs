using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Host
{
    [PublicAPI]
    public static class HostApi
    {
        public const string ApiKey = "HostApi";

        public static IActorRef Create(IActorRefFactory actorRefFactory)
            => actorRefFactory.ActorOf(Props.Create(() => new HostApiManagerActor()));
    }
}