using System;
using System.Linq;
using Akka.Actor;
using Akka.Streams.Implementation.Fusing;
using DotNetty.Common.Utilities;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppCommandProcessor : ReportingActor
    {
        public AppCommandProcessor(IMongoCollection<AppData> apps, GridFSBucket files, IActorRef dataTransfer, RepositoryApi repository)
        {
            Receive<CreateAppCommand>("CreateApp", (command, reporter) =>
            {
                Log.Info("Register Repository");
                var listner = Reporter.CreateListner(Context, reporter, TimeSpan.FromSeconds(20),
                    task => task.PipeTo(Self, 
                        success: t => new ContinueCreateApp(t, command, reporter), 
                        failure: e => new ContinueCreateApp(OperationResult.Failure(e), command, reporter))
                    );

                repository.SendAction(new RegisterRepository(command.TargetRepo, listner) { IgnoreDuplicate = true });
            });

            ReceiveContinue<ContinueCreateApp>("CreateApp2", (command, reporter) =>
            {
                if (!command.Result.Ok)
                {
                    reporter.Compled(OperationResult.Failure(command.Result.Error ?? BuildErrorCodes.CommandErrorRegisterRepository));
                    return;
                }

                var data = apps.AsQueryable().FirstOrDefault(ad => ad.Name == command.Command.AppName);
                if (data != null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.CommandDuplicateApp));
                    return;
                }


            });
        }

        private void CommandPhase1<TCommand>(string name, RepositoryApi api, Func<IActorRef, RepositoryAction> executor, Func<OperationResult, object> result)
            where TCommand : DeploymentCommandBase<TCommand>
        {
            Receive<TCommand>(name, (command, reporter) =>
            {
                var listner = Reporter.CreateListner(Context, reporter, TimeSpan.FromSeconds(20),
                    task => task.PipeTo(Self,
                        success: result,
                        failure: e => result(OperationResult.Failure(e))));
                
                api.SendAction(executor(listner));
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
    }
}