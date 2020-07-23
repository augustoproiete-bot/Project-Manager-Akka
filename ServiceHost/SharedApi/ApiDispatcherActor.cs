using System;
using Akka.Actor;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer;
using ServiceHost.Services;
using Tauron;
using Tauron.Akka;
using Tauron.Application.Master.Commands.Host;
using static Tauron.Application.Master.Commands.Host.InternalHostMessages;

namespace ServiceHost.SharedApi
{
    [UsedImplicitly]
    public sealed class ApiDispatcherActor : ExposedReceiveActor
    {
        public ApiDispatcherActor(IConfiguration configuration, Lazy<IAppManager> appManager, Lazy<IAppRegistry> appRegistry, Lazy<IInstaller> installer)
        {
            var hostName = configuration["applicationName"];

            this.Flow<GetHostName>()
               .From.Func(() => new GetHostNameResult(hostName)).ToSender();

            this.Flow<CommandBase>()
               .From.Action(cb =>
                {
                    switch (cb.Type)
                    {
                        case CommandType.AppManager:
                            appManager.Value.Actor.Forward(cb);
                            break;
                        case CommandType.AppRegistry:
                            appRegistry.Value.Actor.Forward(cb);
                            break;
                        case CommandType.Installer:
                            installer.Value.Actor.Forward(cb);
                            break;
                    }
                });
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery.Get(Context.System)
               .Discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, HostApi.ApiKey));

            base.PreStart();
        }
    }
}