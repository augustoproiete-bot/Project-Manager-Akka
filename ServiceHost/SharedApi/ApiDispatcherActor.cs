using System;
using Akka.Actor;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer;
using ServiceHost.Services;
using Tauron.Akka;
using Tauron.Application.Master.Commands.Administration.Host;
using static Tauron.Application.Master.Commands.Administration.Host.InternalHostMessages;

namespace ServiceHost.SharedApi
{
    [UsedImplicitly]
    public sealed class ApiDispatcherActor : ExposedReceiveActor
    {
        public ApiDispatcherActor(IConfiguration configuration, Lazy<IAppManager> appManager, Lazy<IAppRegistry> appRegistry, Lazy<IInstaller> installer)
        {
            var hostName = configuration["applicationName"];

            Flow<GetHostName>(b => b.Func(() => new GetHostNameResult(hostName)).ToSender());

            Flow<CommandBase>(b => b.Action(cb =>
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
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
            }));
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery.Get(Context.System)
               .Discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, HostApi.ApiKey));

            base.PreStart();
        }
    }
}