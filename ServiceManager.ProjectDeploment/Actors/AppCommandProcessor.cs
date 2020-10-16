﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor, IWithTimers
    {
        private readonly IMongoCollection<AppData> _apps;

        public ITimerScheduler Timers { get; set; } = null!;

        public AppCommandProcessor(IMongoCollection<AppData> apps, GridFSBucket files, RepositoryApi repository, IActorRef dataTransfer,
            IMongoCollection<ToDeleteRevision> toDelete, WorkDistributor<BuildRequest, BuildCompled> workDistributor, IActorRef changeTracker)
        {
            _apps = apps;
            
            CommandPhase1<CreateAppCommand, AppInfo>("CreateApp", repository, 
                (command, listner, reporter) =>
                {
                    reporter.Send(DeploymentMessages.RegisterRepository);
                    return new RegisterRepository(command.TargetRepo, listner()) {IgnoreDuplicate = true};
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

            CommandPhase1<PushVersionCommand, AppBinary>("PushVersion",
                (command, reporter) =>
                {
                    var data = apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.AppName);
                    if (data == null) 
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                    else
                    {
                        string tempFile = Path.Combine(BuildEnv.ApplicationPath, Guid.NewGuid().ToString("D") + ".zip");
                        BuildRequest.SendWork(workDistributor, reporter, data, repository, tempFile)
                            .PipeTo(Self,
                                success: c => new ContinuePushNewVersion(OperationResult.Success((tempFile, c)), command, reporter),
                                failure: e =>
                                {
                                    tempFile.DeleteFile();
                                    return new ContinuePushNewVersion(OperationResult.Failure(e.Unwrap()?.Message ?? "Cancel"), command, reporter);
                                });
                    }
                });

            CommandPhase2<ContinuePushNewVersion, PushVersionCommand, AppBinary>("PushVersion2", (command, result, reporter, data) =>
            {
                if (data == null)
                {
                    if(!reporter.IsCompled)
                        reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                    return null;
                }

                if (!result.Ok)
                    return null;

                using var transaction = apps.Database.Client.StartSession(new ClientSessionOptions{DefaultTransactionOptions = new TransactionOptions(writeConcern:WriteConcern.Acknowledged)});
                var dataFilter = Builders<AppData>.Filter.Eq(ad => ad.Name, data.Name);

                var (fileName, commit) = ((string, string))result.Outcome!;

                try
                {
                    using var targetStream = File.Open(fileName, FileMode.Open);
                    var newId = files.UploadFromStream(data.Name + ".zip", targetStream);

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
                    return new AppBinary(newBinary.Version, newBinary.CreationTime, false, newBinary.Commit, data.Repository);
                }
                finally
                {
                    fileName.DeleteFile();
                }
            });

            CommandPhase1<DeleteAppCommand, AppInfo>("DeleteApp", (command, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(d => d.Name == command.AppName);
                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandAppNotFound));
                    return;
                }

                var transaction = apps.Database.Client.StartSession();
                transaction.StartTransaction();



                transaction.CommitTransaction();
                reporter.Compled(OperationResult.Success(data.ToInfo().IsDeleted()));
            });

            CommandPhase1<ForceBuildCommand, FileTransactionId>("ForceBuild", (command, reporter) =>
            {
                string tempFile = Path.Combine(BuildEnv.ApplicationPath, Guid.NewGuid().ToString("D") + ".zip");
                var tempData = new AppData(command.AppName, -1, DateTime.Now, DateTime.MinValue, command.Repository, command.Repository, ImmutableList<AppFileInfo>.Empty);
                BuildRequest.SendWork(workDistributor, reporter, tempData, repository, tempFile)
                    .PipeTo(Self,
                        success: () => new ContinueForceBuild(OperationResult.Success(tempFile), command, reporter),
                        failure: e =>
                        {
                            tempFile.DeleteFile();
                            return new ContinueForceBuild(OperationResult.Failure(e.Unwrap()?.Message ?? "Cancel"), command, reporter);
                        });
            });

            CommandPhase2<ContinueForceBuild, ForceBuildCommand, FileTransactionId>("ForceBuild2", (command, result, reporter, _) =>
            {
                if (!result.Ok)
                    return null;

                var fileName = (string)result.Outcome!;
                Timers.StartSingleTimer(fileName, fileName, TimeSpan.FromMinutes(10));
            });

            Receive<string>(s => s.DeleteFile());
        }
        
        public sealed class SelfDeleteFile : FileStream
        {
            private readonly string _file;

            public SelfDeleteFile(string file)
                : base(file, FileMode.Open)
                => _file = file;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _file.DeleteFile();
            }

            public override async ValueTask DisposeAsync()
            {
                await base.DisposeAsync();
                _file.DeleteFile();
            }
        }

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

        private void CommandPhase1<TCommand, TResult>(string name, Action<TCommand, Reporter> executor)
            where TCommand : DeploymentCommandBase<TResult>
            => Receive(name, executor);


        private void CommandPhase2<TContinue, TCommand, TResult>(string name, Func<TCommand, OperationResult, Reporter, AppData?, TResult?> executor)
            where TContinue : ContinueCommand<TCommand> 
            where TCommand : DeploymentCommandBase<TResult>
            where TResult : InternalSerializableBase
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

        private sealed class ContinueForceBuild : ContinueCommand<ForceBuildCommand>
        {
            public ContinueForceBuild([NotNull] OperationResult result, ForceBuildCommand command, [NotNull] Reporter reporter) : base(result, command, reporter)
            {
            }
        }
    }
}