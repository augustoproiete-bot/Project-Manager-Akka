using System;
using Akka.Actor;
using DotNetty.Common.Utilities;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkNode.Services;
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
                using var session = apps.Database.Client.StartSession();

            });
        }

        private sealed class ContinueCreateApp : IDelegatingMessage
        {
            public OperationResult Result { get; }

            public CreateAppCommand Command { get; }

            public Reporter Reporter { get; }
            public string Info => Command.AppName;

            public ContinueCreateApp(OperationResult result, CreateAppCommand command, Reporter reporter)
            {
                Result = result;
                Command = command;
                Reporter = reporter;
            }
        }
    }
}