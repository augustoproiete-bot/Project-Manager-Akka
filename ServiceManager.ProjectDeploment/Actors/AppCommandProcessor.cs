using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor, IWithTimers
    {
        private readonly IMongoCollection<AppData> _apps;
        private readonly Dictionary<string, BuildRequest> _pendingBuilds = new Dictionary<string, BuildRequest>();
        private readonly EventSubscribtion _compledSubscription;

        public ITimerScheduler Timers { get; set; } = null!;

        public AppCommandProcessor(IMongoCollection<AppData> apps, GridFSBucket files, IActorRef dataTransfer, RepositoryApi repository, IMongoCollection<ToDeleteRevision> toDelete)
        {
            _apps = apps;

            Receive<CancelRequestInfo>(CancelRequest);
            _compledSubscription = dataTransfer.SubscribeToEvent<TransferCompled>();
            Receive<TransferCompled>(ContinueBuild);


            CommandPhase1<CreateAppCommand, AppInfo>("CreateApp", repository, 
                (command, listner, reporter) =>
                {
                    reporter.Send(DeploymentMessages.RegisterRepository);
                    return new RegisterRepository(command.TargetRepo, listner()) {IgnoreDuplicate = true};
                }, 
                (command, reporter, op) => new ContinueCreateApp(op, command, reporter));

            CommandPhase2<ContinueCreateApp, CreateAppCommand, AppInfo>("CreateApp2", (command, result, reporter, data) =>
            {
                if (data != null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandDuplicateApp));
                    return null;
                }

                var newData = new AppData
                {
                    CreationTime = DateTime.UtcNow,
                    Last = new AppVersion
                    {
                        Version = -1
                    },

                    LastUpdate = DateTime.MinValue,
                    Name = command.AppName,
                    Repository = command.TargetRepo,
                    ProjectName = command.ProjectName
                };
                apps.InsertOne(newData);
                return new AppInfo(newData.Name, newData.Last.Version, newData.LastUpdate.Date, newData.CreationTime, newData.Repository);
            });

            CommandPhase1<PushVersionCommand, AppBinary>("PushVersion", repository,
                (command, listner, reporter) =>
                {
                    var data = apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.AppName);
                    if (data == null)
                    {
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                        return null;
                    }

                    var req = AddRequest(reporter, null);
                    return new TransferRepository(data.Repository, listner(), dataTransfer, req.OperationId);
                }, 
                (command, reporter, result) => new ContinuePushNewVersion(result, command, reporter));
        }

        private void ContinueBuild(TransferCompled obj)
        {
            
        }

        private BuildRequest AddRequest(Reporter source, Func<IDelegatingMessage> nextStep)
        {
            var req = new BuildRequest(source, nextStep);
            _pendingBuilds.Add(req.OperationId, req);
            Timers.StartSingleTimer(req.OperationId, new CancelRequestInfo(req.OperationId), TimeSpan.FromMinutes(10));
            return req;
        }

        private void RequestCompled(string id)
        {
            Timers.Cancel(id);
            _pendingBuilds.Remove(id);
        }

        private void CancelRequest(CancelRequestInfo info) 
            => _pendingBuilds.Remove(info.Id);

        private void CommandPhase1<TCommand, TResult>(string name, RepositoryApi api, Func<TCommand, Func<IActorRef>, Reporter, RepositoryAction?> executor, Func<TCommand, Reporter, OperationResult, object> result)
            where TCommand : DeploymentCommandBase<TResult>
        {
            Receive<TCommand>(name, (command, reporter) =>
            {
                IActorRef CreateListner()
                {
                    return Reporter.CreateListner(Context, reporter, TimeSpan.FromSeconds(20),
                        task => task.PipeTo(Self, Sender,
                            t => result(command, reporter, t),
                            e => result(command, reporter, OperationResult.Failure(e))));

                }

                var msg = executor(command, CreateListner, reporter);
                if (msg == null)
                {
                    if(!reporter.IsCompled)
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.GerneralCommandError));
                    Log.Info("Command Phase 1 {Command} Failed", typeof(TCommand).Name);
                    return;
                }

                Log.Info("Command Phase 1 {Command} -- {Action}", typeof(TCommand).Name, msg.GetType().Name);
                api.SendAction(msg);
            });
        }

        private void CommandPhase2<TContinue, TCommand, TResult>(string name, Func<TCommand, OperationResult, Reporter, AppData?, TResult?> executor)
            where TContinue : ContinueCommand<TCommand> 
            where TCommand : DeploymentCommandBase<TResult>
            where TResult : InternalSerializableBase
        {
            ReceiveContinue<TContinue>(name, (command, reporter) =>
            {
                if (!command.Result.Ok)
                {
                    reporter.Compled(OperationResult.Failure(command.Result.Error ?? BuildErrorCodes.CommandErrorRegisterRepository));
                    return;
                }

                var data = _apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.Command.AppName);
                var result = executor(command.Command, command.Result, command.Reporter, data);
                if (Equals(result, default(TResult)) && !reporter.IsCompled) 
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.GerneralCommandError));
                else
                    reporter.Compled(OperationResult.Success(result));
            });
        }

        //private void CommandPhase2<TContinue, TCommand, TResult>(string name, Func<TContinue, OperationResult, Reporter, AppData?, TResult?> executor)
        //    where TContinue : ContinueCommand<TCommand>
        //    where TCommand : DeploymentCommandBase<TResult>
        //    where TResult : InternalSerializableBase
        //{
        //    ReceiveContinue<TContinue>(name, (command, reporter) =>
        //    {
        //        if (!command.Result.Ok)
        //        {
        //            reporter.Compled(OperationResult.Failure(command.Result.Error ?? BuildErrorCodes.CommandErrorRegisterRepository));
        //            return;
        //        }

        //        var data = _apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.Command.AppName);
        //        var result = executor(command, command.Result, command.Reporter, data);
        //        if (Equals(result, default(TResult)) && !reporter.IsCompled)
        //            reporter.Compled(OperationResult.Failure(BuildErrorCodes.GerneralCommandError));
        //        else
        //            reporter.Compled(OperationResult.Success(result));
        //    });
        //}

        protected override void PostStop()
        {
            _compledSubscription.Dispose();
            base.PostStop();
        }

        private sealed class CancelRequestInfo
        {
            public string Id { get; }

            public CancelRequestInfo(string id) => Id = id;
        }

        private abstract class ContinueCommand<TCommand> : IDelegatingMessage
            where TCommand : IReporterMessage
        {
            public OperationResult Result { get; }

            public TCommand Command { get; }

            public Reporter Reporter { get; }
            public string Info => Command.Info;

            protected ContinueCommand(OperationResult result, TCommand command, Reporter reporter)
            {
                Result = result;
                Command = command;
                Reporter = reporter;
            }
        }

        private sealed class ContinueCreateApp : ContinueCommand<CreateAppCommand>
        {

            public ContinueCreateApp(OperationResult result, CreateAppCommand command, Reporter reporter)
                : base(result, command, reporter)
            {
            }
        }

        private sealed class ContinuePushNewVersion : ContinueCommand<PushVersionCommand>
        {
            public ContinuePushNewVersion([NotNull] OperationResult result, PushVersionCommand command, [NotNull] Reporter reporter) 
                : base(result, command, reporter)
            {
            }
        }

        private sealed class BuildFinishPushNewVersion : ContinueCommand<PushVersionCommand>
        {
            public BuildFinishPushNewVersion([NotNull] OperationResult result, PushVersionCommand command, [NotNull] Reporter reporter) 
                : base(result, command, reporter)
            {
            }
        }
    }
}