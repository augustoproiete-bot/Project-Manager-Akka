using System;
using System.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;

namespace ServiceHost.Services.Impl
{
    [UsedImplicitly]
    public sealed class AppManagerActor : ExposedReceiveActor
    {
        private readonly IAppRegistry _appRegistry;

        public AppManagerActor(IAppRegistry appRegistry, IInstaller installer)
        {
            _appRegistry = appRegistry;
            var ability = new SubscribeAbility(this);

            installer.Actor
               .Tell(new EventSubscribe(typeof(InstallerationCompled)));

            Receive<InstallerationCompled>(ic =>
            {
                void PipeToSelf(string name)
                {
                    appRegistry.Actor
                       .Ask<InstalledAppRespond>(new InstalledAppQuery(name), TimeSpan.FromSeconds(10))
                       .PipeTo(Self, success: ar => new StartApp(ar.App));
                }

                if(ic.InstallAction != InstallationAction.Install || !ic.Succesfull) return;

                switch (ic.Type)
                {
                    case AppType.Cluster:
                        Cluster.Get(Context.System).RegisterOnMemberUp(() => PipeToSelf(ic.Name));
                        break;
                    case AppType.StartUp:
                        PipeToSelf(ic.Name);
                        break;
                }
            });

            this.Flow<StopApps>()
               .From.Action(() => Context.GetChildren().Foreach(r => r.Tell(new InternalStopApp())))
               .AndReceive();

            this.Flow<StopApp>()
               .From.Action(s =>
                {
                    var child = Context.Child(s.Name);
                    if (child.IsNobody())
                        Sender.Tell(new StopResponse(s.Name));
                    else
                        child.Forward(new InternalStopApp());
                })
               .AndReceive();

            this.Flow<StopResponse>()
               .From.Action(r => ability.Send(r))
               .AndReceive();

            this.Flow<StartApps>()
               .From.Action(sa => 
                    appRegistry.Actor
                   .Ask<AllAppsResponse>(new AllAppsQuery())
                   .PipeTo(Self, Sender, r => new InternalFilterApps(sa.AppType, r.Apps)))
               .AndRespondTo<InternalFilterApps>().Action(fa => fa.Names.Foreach(s => Self.Tell(new InternalFilterApp(fa.AppType, s))))
               .AndRespondTo<InternalFilterApp>().Action(fa => 
                    appRegistry.Actor
                   .Ask<InstalledAppRespond>(new InstalledAppQuery(fa.Name))
                   .ContinueWith(t =>
                    {
                        if(!t.IsCompletedSuccessfully && t.Result.Fault && t.Result.App.IsEmpty())
                            return;

                        var data = t.Result.App;
                        if(data.AppType != fa.AppType)
                            return;

                        Self.Tell(new StartApp(data));
                    }))
               .AndRespondTo<StartApp>().Action(sa =>
                {
                    if(sa.App.IsEmpty()) return;
                    if (!Context.Child(sa.App.Name).IsNobody())
                        return;

                    Context.ActorOf(Props.Create<AppProcessActor>(sa.App)).Tell(new InternalStartApp());
                })
               .AndReceive();

            Receive<Status.Failure>(f => Log.Error(f.Cause, "Error while processing message"));

            ability.MakeReceive();
        }

        protected override void PreStart()
        {
            _appRegistry.Actor.Tell(new EventSubscribe(typeof(RegistrationResponse)));
            CoordinatedShutdown
               .Get(Context.System)
               .AddTask(CoordinatedShutdown.PhaseBeforeServiceUnbind, "AppManagerShutdown", new ContextShutdown(Log, Context).HostShutdown);

            base.PreStart();
        }

        private sealed class InternalFilterApps
        {
            public AppType AppType { get; }

            public string[] Names { get; }

            public InternalFilterApps(AppType appType, string[] names)
            {
                AppType = appType;
                Names = names;
            }
        }

        private sealed class InternalFilterApp
        {
            public AppType AppType { get; }

            public string Name { get; }

            public InternalFilterApp(AppType appType, string name)
            {
                AppType = appType;
                Name = name;
            }
        }

        private sealed class ContextShutdown
        {
            private readonly ILoggingAdapter _log;
            private readonly IUntypedActorContext _context;

            public ContextShutdown(ILoggingAdapter log, IUntypedActorContext context)
            {
                _log = log;
                _context = context;
            }

            public Task<Done> HostShutdown()
            {
                _log.Info("Shutdown All Host Apps");
                return Task.WhenAll(_context
                   .GetChildren()
                   .Select(ar => ar.Ask<StopResponse>(new InternalStopApp(), TimeSpan.FromMinutes(2)))
                   .ToArray()).ContinueWith(_ => Done.Instance);
            }
        }
    }
}