using Akka.Actor;
using Akka.DI.Core;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceHost.SharedApi
{
    public sealed class ApiDispatcherStartup : IStartUpAction
    {
        private readonly ActorSystem _system;

        public ApiDispatcherStartup(ActorSystem system) 
            => _system = system;

        public void Run() 
            => _system.ActorOf(_system.DI().Props<ApiDispatcherActor>(), "Api-Dispatcher");
    }
}