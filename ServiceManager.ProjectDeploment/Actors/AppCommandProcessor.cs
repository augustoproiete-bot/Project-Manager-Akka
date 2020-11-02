using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.AkkNode.Services.Commands;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Operations;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor, IWithTimers
    {
        private readonly IMongoCollection<AppData> _apps;

        public ITimerScheduler Timers { get; set; } = null!;

        public AppCommandProcessor(IMongoCollection<AppData> apps, GridFSBucket files, RepositoryApi repository, DataTransferManager dataTransfer,
            IMongoCollection<ToDeleteRevision> toDelete, WorkDistributor<BuildRequest, BuildCompled> workDistributor, IActorRef changeTracker)
        {
            _apps = apps;
            
            CommandPhase1<CreateAppCommand>("CreateApp", repository, 
                (command, reporter) =>
                {
                    reporter.Send(DeploymentMessages.RegisterRepository);
                    return new RegisterRepository(command.TargetRepo) {IgnoreDuplicate = true};
                }, 
                (command, reporter, op) => new ContinueCreateApp(op, command, reporter));

            CommandPhase2<ContinueCreateApp, CreateAppCommand, AppInfo>("CreateApp2", (command, result, reporter, data) =>
            {
                if (!result.Ok)
                {
                    if (reporter.IsCompled) return null;
                    reporter.Compled(OperationResult.Failure(result.Error ?? BuildErrorCodes.CommandErrorRegisterRepository));
                    return null;
                }

                if (data != null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandDuplicateApp));
                    return null;
                }

                var newData = new AppData(command.AppName, -1, DateTime.UtcNow, DateTime.MinValue, command.TargetRepo, command.ProjectName, ImmutableList<AppFileInfo>.Empty);
                
                apps.InsertOne(newData);
                var info = newData.ToInfo();

                changeTracker.Tell(info);
                return info;
            });

            CommandPhase1<PushVersionCommand>("PushVersion",
                (command, reporter) =>
                {
                    var data = apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.AppName);
                    if (data == null) 
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                    else
                    {
                        BuildRequest.SendWork(workDistributor, reporter, data, repository, BuildEnv.TempFiles.CreateFile())
                            .PipeTo(Self,
                                success: c => new ContinuePushNewVersion(OperationResult.Success(c), command, reporter),
                                failure: e => new ContinuePushNewVersion(OperationResult.Failure(e.Unwrap()?.Message ?? "Cancel"), command, reporter));
                    }
                });

            CommandPhase2<ContinuePushNewVersion, PushVersionCommand, AppBinary>("PushVersion2", (command, result, reporter, data) =>
            {
                if (data == null)
                {
                    if (!reporter.IsCompled)
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                    return null;
                }

                if (!result.Ok)
                    return null;

                using var transaction = apps.Database.Client.StartSession(new ClientSessionOptions {DefaultTransactionOptions = new TransactionOptions(writeConcern: WriteConcern.Acknowledged)});
                var dataFilter = Builders<AppData>.Filter.Eq(ad => ad.Name, data.Name);

                var (commit, fileName) = ((string, ITempFile)) result.Outcome!;

                using var targetStream = fileName;
                
                var newId = files.UploadFromStream(data.Name + ".zip", targetStream.Stream);

                var newBinary = new AppFileInfo(newId, data.Last + 1, DateTime.UtcNow, false, commit);
                var newBinarys = data.Versions.Add(newBinary);

                var definition = Builders<AppData>.Update;
                var updates = new List<UpdateDefinition<AppData>>
                              {
                                  definition.Set(ad => ad.Last, newBinary.Version),
                                  definition.Set(ad => ad.Versions, newBinarys)
                              };

                var deleteUpdates = new List<ToDeleteRevision>();

                if (data.Versions.Count(s => !s.Deleted) > 5)
                {
                    foreach (var info in newBinarys.OrderByDescending(i => i.CreationTime).Skip(5))
                    {
                        if (info.Deleted) continue;
                        info.Deleted = true;
                        deleteUpdates.Add(new ToDeleteRevision(info.File.ToString()));
                    }
                }

                transaction.StartTransaction();

                if (deleteUpdates.Count != 0)
                    toDelete.InsertMany(transaction, deleteUpdates);
                if (!apps.UpdateOne(transaction, dataFilter, definition.Combine(updates)).IsAcknowledged)
                {
                    transaction.AbortTransaction();
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.DatabaseError));
                    return null;
                }

                transaction.CommitTransaction();

                changeTracker.Tell(_apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.AppName));
                return new AppBinary(command.AppName, newBinary.Version, newBinary.CreationTime, false, newBinary.Commit, data.Repository);
            });

            CommandPhase1<DeleteAppCommand>("DeleteApp", (command, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(d => d.Name == command.AppName);
                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                    return;
                }

                var transaction = apps.Database.Client.StartSession();
                transaction.StartTransaction();

                var arr = data.Versions.Where(f => f.Deleted).ToArray();
                if(arr.Length > 0)
                    toDelete.InsertMany(transaction, arr.Select(f => new ToDeleteRevision(f.File.ToString())));
                apps.DeleteOne(transaction, Builders<AppData>.Filter.Eq(a => a.Name, data.Name));

                transaction.CommitTransaction();
                reporter.Compled(OperationResult.Success(data.ToInfo().IsDeleted()));
            });

            CommandPhase1<ForceBuildCommand>("ForceBuild", (command, reporter) =>
            {
                var tempData = new AppData(command.AppName, -1, DateTime.Now, DateTime.MinValue, command.Repository, command.Repository, ImmutableList<AppFileInfo>.Empty);
                BuildRequest.SendWork(workDistributor, reporter, tempData, repository, BuildEnv.TempFiles.CreateFile())
                    .PipeTo(Self,
                        success: d => new ContinueForceBuild(OperationResult.Success(d.Item2), command, reporter),
                        failure: e => new ContinueForceBuild(OperationResult.Failure(e.Unwrap()?.Message ?? "Cancel"), command, reporter));
            });

            CommandPhase2<ContinueForceBuild, ForceBuildCommand, FileTransactionId>("ForceBuild2", (command, result, reporter, _) =>
            {
                if (!result.Ok || command.Manager == null)
                    return null;

                if (!(result.Outcome is TempStream target)) return null;

                var request = DataTransferRequest.FromStream(target, command.Manager);
                dataTransfer.Request(request);

                return new FileTransactionId(request.OperationId);

            });
        }
        
        private void CommandPhase1<TCommand>(string name, RepositoryApi api, Func<TCommand, Reporter, IReporterMessage?> executor, Func<TCommand, Reporter, IOperationResult, object> result)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
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

                var msg = executor(command, reporter);
                if (msg == null)
                {
                    if(!reporter.IsCompled)
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.GerneralCommandError));
                    Log.Info("Command Phase 1 {Command} Failed", typeof(TCommand).Name);
                    return;
                }

                Log.Info("Command Phase 1 {Command} -- {Action}", typeof(TCommand).Name, msg.GetType().Name);
                msg.SetListner(CreateListner());
                ((ISender)api).SendCommand(msg);
            });
        }

        private void CommandPhase1<TCommand>(string name, Action<TCommand, Reporter> executor)
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            => Receive(name, executor);


        private void CommandPhase2<TContinue, TCommand, TResult>(string name, Func<TCommand, IOperationResult, Reporter, AppData?, TResult?> executor)
            where TContinue : ContinueCommand<TCommand> 
            where TCommand : ReporterCommandBase<DeploymentApi, TCommand>, IDeploymentCommand
            where TResult : class
        {
            ReceiveContinue<TContinue>(name, (command, reporter) =>
            {
                var data = _apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.Command.AppName);

                var result = executor(command.Command, command.Result, command.Reporter, data);

                if(reporter.IsCompled) return;

                if (Equals(result, default(TResult)) && !reporter.IsCompled) 
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.GerneralCommandError));
                else
                    reporter.Compled(OperationResult.Success(result));
            });
        }
        
        private abstract class ContinueCommand<TCommand> : IDelegatingMessage
            where TCommand : IReporterMessage
        {
            public IOperationResult Result { get; }

            public TCommand Command { get; }

            public Reporter Reporter { get; }
            public string Info => Command.Info;

            protected ContinueCommand(IOperationResult result, TCommand command, Reporter reporter)
            {
                Result = result;
                Command = command;
                Reporter = reporter;
            }
        }

        private sealed class ContinueCreateApp : ContinueCommand<CreateAppCommand>
        {

            public ContinueCreateApp(IOperationResult result, CreateAppCommand command, Reporter reporter)
                : base(result, command, reporter)
            {
            }
        }

        private sealed class ContinuePushNewVersion : ContinueCommand<PushVersionCommand>
        {
            public ContinuePushNewVersion([NotNull] IOperationResult result, PushVersionCommand command, [NotNull] Reporter reporter) 
                : base(result, command, reporter)
            {
            }
        }

        private sealed class ContinueForceBuild : ContinueCommand<ForceBuildCommand>
        {
            public ContinueForceBuild([NotNull] IOperationResult result, ForceBuildCommand command, [NotNull] Reporter reporter) : base(result, command, reporter)
            {
            }
        }
    }
}