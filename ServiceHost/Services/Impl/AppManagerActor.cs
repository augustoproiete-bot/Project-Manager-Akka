using System;
using System.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;

namespace ServiceHost.Services.Impl
{
    [UsedImplicitly]
    public sealed class AppManagerActor : ExposedReceiveActor
    {
        private readonly IAppRegistry _appRegistry;

        public AppManagerActor(IAppRegistry appRegistry)
        {
            _appRegistry = appRegistry;

            
        }

        protected override void PreStart()
        {
            _appRegistry.Actor.Tell(new EventSubscribe(typeof(RegistrationResponse)));
            CoordinatedShutdown
               .Get(Context.System)
               .AddTask(CoordinatedShutdown.PhaseBeforeServiceUnbind, "AppManagerShutdown", HostShutdown);

            base.PreStart();
        }

        private Task<Done> HostShutdown()
        {
            Log.Info("Shutdown All Host Apps");
            return Task.WhenAll(Context
               .GetChildren()
               .Select(ar => ar.Ask<InternalStopResponse>(new InternalStopApp(), TimeSpan.FromMinutes(2)))
               .ToArray()).ContinueWith(_ => Done.Instance);
        }
    }
}