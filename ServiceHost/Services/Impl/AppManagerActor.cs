using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Services.Impl
{
    [UsedImplicitly]
    public sealed class AppManagerActor : ExposedReceiveActor, IWithTimers
    {
        private readonly IAppRegistry _appRegistry;

        public ITimerScheduler Timers { get; set; } = null!;

        public AppManagerActor(IAppRegistry appRegistry, IInstaller installer, InstallChecker checker)
        {
            _appRegistry = appRegistry;
            var ability = new SubscribeAbility(this);

            installer.Actor
               .Tell(new EventSubscribe(typeof(InstallerationCompled)));

            Receive<UpdateTitle>(_ => Console.Title = "Application Host");

            Receive<InstallerationCompled>(ic =>
            {
                if (checker.IsInstallationStart) 
                    Context.System.Terminate();

                void PipeToSelf(string name)
                {
                    appRegistry.Actor
                       .Ask<InstalledAppRespond>(new InstalledAppQuery(name), TimeSpan.FromSeconds(10))
                       .PipeTo(Self, success: ar => new StartApp(ar.App));
                }

                if(ic.InstallAction != InstallationAction.Install || !ic.Succesfull) return;

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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

            Flow<StopApps>(b => b.Action(() => Context.GetChildren().Foreach(r => r.Tell(new InternalStopApp()))));

            Flow<StopApp>(b =>
                b.Action(s =>
                {
                    var child = Context.Child(s.Name);
                    if (child.IsNobody())
                        Sender.Tell(new StopResponse(s.Name, false));
                    else
                        child.Forward(new InternalStopApp());
                }));

            Flow<StopResponse>(b => b.Action(r => ability.Send(r)));

            Flow<StartApps>(b =>
            {
                b.Action(sa =>
                        appRegistry.Actor
                           .Ask<AllAppsResponse>(new AllAppsQuery())
                           .PipeTo(Self, Sender, r => new InternalFilterApps(sa.AppType, r.Apps)))
                   .Then<InternalFilterApps>(b1 => b1.Action(fa => fa.Names.Foreach(s => Self.Tell(new InternalFilterApp(fa.AppType, s)))))
                   .Then<InternalFilterApp>(
                        b1 => b1.Action(fa =>
                            appRegistry.Actor
                               .Ask<InstalledAppRespond>(new InstalledAppQuery(fa.Name))
                               .ContinueWith(t =>
                                {
                                    if (!t.IsCompletedSuccessfully && t.Result.Fault && t.Result.App.IsEmpty())
                                        return;

                                    var data = t.Result.App;
                                    if (data.AppType != fa.AppType)
                                        return;

                                    Self.Tell(new StartApp(data));
                                })))
                   .Then<StartApp>(b1 => b1.Action(sa =>
                    {
                        if (sa.App.IsEmpty()) return;
                        if (!Context.Child(sa.App.Name).IsNobody())
                            return;

                        Context.ActorOf(Props.Create<AppProcessActor>(sa.App)).Tell(new InternalStartApp());
                    }));
            });

            Receive<Status.Failure>(f => Log.Error(f.Cause, "Error while processing message"));

            #region SharedApi

            Receive<StopAllApps>(_ =>
            {
                Task.WhenAll(Context.GetChildren()
                       .Select(ar => ar.Ask<StopResponse>(new InternalStopApp(), TimeSpan.FromMinutes(0.7))).ToArray())
                   .ContinueWith(t => new OperationResponse(t.IsCompletedSuccessfully && t.Result.All(s => !s.Error)))
                   .PipeTo(Sender, failure: e =>
                    {
                        Log.Warning(e, "Error on Shared Api Stop All Apps");
                        return new OperationResponse(false);
                    });
            });

            Flow<StartAllApps>(b => b.Func(_ =>
            {
                Self.Tell(new StartApps(AppType.Cluster));
                return new OperationResponse(true);
            }).ToSender());

            Receive<QueryAppStaus>(s =>
            {
                var childs = Context.GetChildren().ToArray();

                Task.Run(async () =>
                {
                    List<AppProcessActor.GetNameResponse> names = new List<AppProcessActor.GetNameResponse>();

                    foreach (var actorRef in childs)
                    {
                        try
                        {
                            names.Add(await actorRef.Ask<AppProcessActor.GetNameResponse>(new AppProcessActor.GetName(), TimeSpan.FromSeconds(5)));
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error on Recive Prcess Apps Name");
                        }
                    }

                    return names.ToArray();
                }).PipeTo(Sender, 
                    success:arr => new AppStatusResponse(s.OperationId, arr.ToImmutableDictionary(g => g.Name, g => g.Running)),
                    failure:e =>
                    {
                        Log.Error(e, "Error getting Status");
                        return new AppStatusResponse(s.OperationId);
                    });
            });

            Receive<StopHostApp>(sha =>
            {
                var pm = Context.Child(sha.AppName);
                if (pm.IsNobody())
                    Context.Sender.Tell(new OperationResponse(false));
                pm.Ask<StopResponse>(new InternalStopApp(), TimeSpan.FromMinutes(1))
                   .PipeTo(Sender,
                        success:() => new OperationResponse(true),
                        failure: e =>
                        {
                            Log.Warning(e, "Error on Shared Api Stop");
                            return new OperationResponse(false);
                        });
            });

            Receive<StartHostApp>
            (sha =>
            {
                if (string.IsNullOrWhiteSpace(sha.AppName))
                    Sender.Tell(new OperationResponse(false));
                var pm = Context.Child(sha.AppName);
                if (pm.IsNobody())
                {
                    var self = Self;
                    appRegistry.Actor.Ask<InstalledAppRespond>(new InstalledAppQuery(sha.AppName), TimeSpan.FromMinutes(1))
                       .ContinueWith(t =>
                        {
                            if (t.Result.Fault)
                                return new OperationResponse(false);

                            self.Tell(new StartApp(t.Result.App));
                            return new OperationResponse(true);
                        })
                       .PipeTo(Sender,
                            failure: e =>
                            {
                                Log.Warning(e, "Error on Shared Api Stop");
                                return new OperationResponse(false);
                            });
                }
                else
                {
                    pm.Tell(new InternalStartApp());
                    Sender.Tell(new OperationResponse(true));
                }
            });

            #endregion

            ability.MakeReceive();
        }

        protected override void PreStart()
        {
            Timers.StartPeriodicTimer(new object(), new UpdateTitle(), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

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

        private sealed class UpdateTitle
        {
            
        }
    }
}