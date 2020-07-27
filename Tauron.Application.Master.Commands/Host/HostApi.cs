using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Host
{
    [PublicAPI]
    public sealed class HostApi
    {
        public const string ApiKey = "HostApi";

        public static HostApi Create(IActorRefFactory actorRefFactory)
            => new HostApi(actorRefFactory.ActorOf(Props.Create(() => new HostApiManagerActor())));

        private readonly IActorRef _actorRef;

        public HostApi(IActorRef actorRef) => _actorRef = actorRef;
    }
}