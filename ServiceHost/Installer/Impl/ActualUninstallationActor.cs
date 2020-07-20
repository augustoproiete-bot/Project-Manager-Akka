using System;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Services;
using Tauron.Akka;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Workflow;

namespace ServiceHost.Installer.Impl
{
    [UsedImplicitly]
    public sealed class ActualUninstallationActor : LambdaWorkflowActor<UnistallContext>
    {
        private readonly StepId Stopping = new StepId(nameof(Stopping));
        private readonly StepId Unistall = new StepId(nameof(Unistall));
        private readonly StepId Finalization = new StepId(nameof(Finalization));

        public ActualUninstallationActor(IAppRegistry registry, IAppManager manager, IConfiguration configuration)
        {
            string appBaseLocation = configuration["AppsLocation"];

            WhenStep(StepId.Start, config =>
            {
                config.OnExecute(context =>
                {
                    registry.Ask<InstalledAppRespond>(new InstalledAppQuery(context.Name), TimeSpan.FromSeconds(15))
                       .PipeTo(Self);

                    return StepId.Waiting;
                });

                Signal<InstalledAppRespond>((context, respond) =>
                {
                    if (respond.Fault)
                    {
                        SetError(ErrorCodes.QueryAppInfo);
                        return StepId.Fail;
                    }

                    context.App = respond.App;
                    return Stopping;
                });
            });

            WhenStep(Stopping, config =>
            {
                Signal<StopResponse>((context, response) =>
                {
                });
            });

            Signal<Failure>((ctx, f) =>
            {
                SetError(f.Exception.Message);
                return StepId.Fail;
            });

            OnFinish(wr =>
            {
                if (!wr.Succesfully)
                {
                    Log.Warning("Installation Failed Recover {App}", wr.Context.Name);
                    wr.Context.Recovery.Recover(Log);
                }

                var finish = new InstallerationCompled(wr.Succesfully, wr.Error, wr.Context.App.AppType, wr.Context.Name, InstallationAction.Uninstall);
                if (!Sender.Equals(Context.System.DeadLetters))
                    Sender.Tell(finish, ActorRefs.NoSender);

                Context.Parent.Tell(finish);
                Context.Stop(Self);
            });
        }
    }
}