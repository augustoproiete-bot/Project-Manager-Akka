using System;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Host
{
    [PublicAPI]
    public sealed class HostApi
    {
        public const string ApiKey = "HostApi";

        public static HostApi Create(IActorRefFactory actorRefFactory)
            => new HostApi(actorRefFactory.ActorOf(Props.Create(() => new HostApiManagerActor())));

        private readonly IActorRef _actorRef;
        
        public HostApi(IActorRef actorRef) 
            => _actorRef = actorRef;

        public Task<OperationResponse> ExecuteCommand(InternalHostMessages.CommandBase command)
            => _actorRef.Ask<OperationResponse>(command, TimeSpan.FromMinutes(2));

        public EventSubscribtion Event<T>() => _actorRef.SubscribeToEvent<T>();
    }
}