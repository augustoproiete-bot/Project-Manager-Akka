using System;
using System.IO;
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
        private static readonly StepId Stopping = new StepId(nameof(Stopping));
        private static readonly StepId Unistall = new StepId(nameof(Unistall));
        private static readonly StepId Finalization = new StepId(nameof(Finalization));

        public ActualUninstallationActor(IAppRegistry registry, IAppManager manager)
        {
            WhenStep(StepId.Start, config =>
            {
                config.OnExecute(context =>
                {
                    Log.Info("Start Unistall Apps {Name}", context.Name);
                    registry.Ask<InstalledAppRespond>(new InstalledAppQuery(context.Name), TimeSpan.FromSeconds(15))
                       .PipeTo(Self);

                    return StepId.Waiting;
                });

                Signal<InstalledAppRespond>((context, respond) =>
                {
                    if (respond.Fault || respond.App.IsEmpty())
                    {
                        Log.Warning("Error on Query Application Info {Name}", context.Name);
                        SetError(ErrorCodes.QueryAppInfo);
                        return StepId.Fail;
                    }

                    context.App = respond.App;
                    return Stopping;
                });
            });

            WhenStep(Stopping, config =>
            {
                config.OnExecute(context =>
                {
                    Log.Info("Stoping Appliocation {Name}", context.Name);
                    manager.Actor
                       .Ask<StopResponse>(new StopApp(context.Name), TimeSpan.FromMinutes(1))
                       .PipeTo(Self);
                    return StepId.Waiting;
                });

                Signal<StopResponse>((context, response) => Unistall);
            });

            WhenStep(Unistall, config =>
            {
                config.OnExecute((context, step) =>
                {
                    try
                    {
                        Log.Info("Backup Application {Name}", context.Name);
                        context.Backup.Make(context.App.Path);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error while making Backup");
                        step.ErrorMessage = e.Message;
                        return StepId.Fail;
                    }

                    context.Recovery.Add(context.Backup.Recover);

                    try
                    {
                        Log.Info("Delete Application Directory {Name}", context.Name);
                        Directory.Delete(context.App.Path, true);
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Error while Deleting Apps Directory");
                    }

                    return Finalization;
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
                    Log.Warning("Installation Failed Recover {Apps}", wr.Context.Name);
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