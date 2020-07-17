﻿using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl.Source;
using Tauron;
using Tauron.Application.ActorWorkflow;
using Tauron.Application.Workflow;

namespace ServiceHost.Installer.Impl
{
    [UsedImplicitly]
    public sealed class ActualInstallerActor : LambdaWorkflowActor<InstallerContext>
    {
        private static readonly StepId Preperation = new StepId(nameof(Preperation));
        private static readonly StepId Validation = new StepId(nameof(Validation));
        private static readonly StepId PreCopy = new StepId(nameof(PreCopy));
        private static readonly StepId Copy = new StepId(nameof(Copy));
        private static readonly StepId Registration = new StepId(nameof(Registration));
        private static readonly StepId Finalization = new StepId(nameof(Finalization));

        public ActualInstallerActor(IAppRegistry registry, IConfiguration configuration)
        {
            string appBaseLocation = configuration["AppsLocation"];

            StartMessage<FileInstallationRequest>(HandleFileInstall);

            WhenStep(Preperation, config =>
            {
                config.OnExecute((context, step) =>
                {
                    Log.Info("Perpering Data for Installation: {Name}", context.Name);
                    return context.SetSource(InstallationSourceSelector.Select, step.SetError)
                       .When(i => i != EmptySource.Instnace, () =>
                                StepId.Waiting.DoAnd(_ =>
                                    registry.Actor
                                       .Ask<InstalledAppRespond>(new InstalledAppQuery(context.Name), TimeSpan.FromSeconds(5))
                                       .PipeTo(Self))
                          , StepId.Fail);
                });

                Signal<InstalledAppRespond>((context, respond) =>
                {
                    if (respond.Fault)
                    {
                        SetError(ErrorCodes.QueryAppInfo);
                        return StepId.Fail;
                    }

                    return Validation.DoAnd(_ => context.SetInstalledApp(respond.App));
                });
            });

            WhenStep(Validation, confg =>
            {
                confg.OnExecute((context, step) =>
                {
                    Log.Info("Validating Data for installation: {Name}", context.Name);
                    if (context.Source.ValidateInput(context) is Status.Failure failure)
                    {
                        Log.Warning(failure.Cause, "Source Validation Failed {Name}", context.Name);
                        step.ErrorMessage = failure.Cause.Message;
                        return StepId.Fail;
                    }

                    // ReSharper disable once InvertIf
                    if (!context.InstalledApp.IsEmpty() && context.InstalledApp.Name == context.Name && !context.Override)
                    {
                        Log.Warning("App is Installed {Name}", context.Name);
                        step.ErrorMessage = ErrorCodes.ExistingApp;
                        return StepId.Fail;
                    }

                    return PreCopy;
                });
            });

            WhenStep(PreCopy, config =>
            {
                config.OnExecute((context, step) =>
                {
                    Log.Info("Prepare for Copy Data {Name}", context.Name);
                    string targetAppPath = Path.GetFullPath(Path.Combine(appBaseLocation, context.Name));
                    try
                    {
                        if (!Directory.Exists(targetAppPath))
                            Directory.CreateDirectory(targetAppPath);

                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Installation Faild during Directory Creation {Name}", context.Name);
                        step.ErrorMessage = ErrorCodes.DirectoryCreation;
                        return StepId.Fail;
                    }

                    context.InstallationPath = targetAppPath;
                    context.Source.PreperforCopy(context)
                       .PipeTo(Self, success: () => new PreCopyCompled());

                    if (context.Override)
                    {
                        context.Backup.Make(targetAppPath);
                        context.Recovery.Add(context.Backup.Recover);
                    }

                    return StepId.Waiting;
                });

                Signal<PreCopyCompled>((c, m) => Copy);
            });

            WhenStep(Copy, config =>
            {
                config.OnExecute((context, step) =>
                {
                    Log.Info("Copy Application Data {Name}", context.Name);
                    context.Recovery.Add(log =>
                    {
                        log.Info("Clearing Installation Directory during Recover {Name}", context.Name);
                        context.InstallationPath.ClearDirectory();
                    });

                    try
                    {
                        context.Source.CopyTo(context, context.InstallationPath)
                           .PipeTo(Self, success: () => new CopyCompled());
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error on Extracting Files to Directory {Name}", context.Name);
                        step.ErrorMessage = e.Message;
                        return StepId.Fail;
                    }

                    context.Recovery.Add(log =>
                    {
                        log.Info("Delete Insttalation Files during Recovery {Name}", context.Name);
                        context.InstallationPath.ClearDirectory();
                    });

                    return StepId.Waiting;
                });

                Signal<CopyCompled>((context, compled) => Registration);
            });

            WhenStep(Registration, config =>
            {
                config.OnExecute((context, step) =>
                {
                    Log.Info("Register Application for Host {Name}", context.Name);

                    if (context.InstalledApp.IsEmpty())
                    {
                        registry.Actor
                           .Ask<RegistrationResponse>(new NewRegistrationRequest(context.Name, context.InstallationPath, context.Source.Version, context.AppType), TimeSpan.FromSeconds(15))
                           .PipeTo(Self);
                    }
                    else
                    {
                        registry.Actor
                           .Ask<RegistrationResponse>(new UpdateRegistrationRequest(context.Name), TimeSpan.FromSeconds(15))
                           .PipeTo(Self);
                    }

                    return StepId.Waiting;
                });
                
                Signal<RegistrationResponse>((context, response) =>
                {
                    if (response.Scceeded)
                        return Finalization;
                    SetError(response.Error?.Message ?? "");
                    return StepId.Fail;
                });
            });

            WhenStep(Finalization, config =>
            {
                config.OnExecute(context =>
                {
                    Log.Info("Clean Up and Compleding {Name}", context.Name);

                    context.Backup.CleanUp();

                    try
                    {
                        context.Source.CleanUp(context);
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Error on Clean Up {Name}", context.Name);
                    }

                    return StepId.Finish;
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
                    Log.Warning("Installation Failed Recover {Name}", wr.Context.Name);
                    wr.Context.Recovery.Recover(Log);
                }

                Sender.Tell(new InstallerationCompled(wr.Succesfully, wr.Error), ActorRefs.NoSender);
                Context.Stop(Self);
            });
        }

        private void HandleFileInstall(FileInstallationRequest request) 
            => Start(new InstallerContext(InstallType.Manual, request.Name, request.Path,  request.Override, request.AppType));

        private sealed class PreCopyCompled
        {
            
        }

        private sealed class CopyCompled
        {
            
        }
    }
}