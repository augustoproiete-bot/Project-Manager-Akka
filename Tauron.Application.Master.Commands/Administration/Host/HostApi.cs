using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    [PublicAPI]
    public sealed class HostApi
    {
        public const string ApiKey = "HostApi";

        private static object _lock = new object();

        private static HostApi? _hostApi;

        public static HostApi CreateOrGet(ActorSystem actorRefFactory)
        {
            lock (_lock)
                return _hostApi ??= new HostApi(actorRefFactory.ActorOf(HostApiManagerActor.CreateProps()));
        }

        private readonly IActorRef _actorRef;
        
        public HostApi(IActorRef actorRef) 
            => _actorRef = actorRef;

        public Task<OperationResponse> ExecuteCommand(InternalHostMessages.CommandBase command)
            => _actorRef.Ask<OperationResponse>(command, TimeSpan.FromMinutes(2));

        public Task<ImmutableList<HostApp>> QueryApps(string name)
            => _actorRef
               .Ask<HostAppsResponse>(new QueryHostApps(name), TimeSpan.FromSeconds(30))
               .ContinueWith(t => t.Result.Apps);

        public EventSubscribtion Event<T>() => _actorRef.SubscribeToEvent<T>();
    }
}