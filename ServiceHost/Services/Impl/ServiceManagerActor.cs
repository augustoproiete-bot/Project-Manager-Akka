using System;
using System.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;

namespace ServiceHost.Services.Impl
{
    [UsedImplicitly]
    public sealed class ServiceManagerActor : ExposedReceiveActor
    {
        private readonly IAppRegistry _appRegistry;

        public ServiceManagerActor(IAppRegistry appRegistry)
        {
            _appRegistry = appRegistry;
        }

        protected override void PreStart()
        {
            _appRegistry.Actor.Tell(new EventSubscribe(typeof(RegistrationResponse)));
            CoordinatedShutdown
               .Get(Context.System)
               .AddTask(CoordinatedShutdown.PhaseBeforeServiceUnbind, "ServiceManagerShutdown", HostShutdown);

            base.PreStart();
        }

        private Task<Done> HostShutdown()
        {
            return Task.WhenAll(Context
               .GetChildren()
               .Select(ar => ar.Ask<InternalStopResponse>(new InternalStopService(), TimeSpan.FromMinutes(2)))
               .ToArray()).ContinueWith(_ => Done.Instance);
        }
    }
}