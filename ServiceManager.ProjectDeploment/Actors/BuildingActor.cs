using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Akka.Actor;
using ServiceManager.ProjectDeployment.Build;
using ServiceManager.ProjectDeployment.Data;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Actors
{
    public enum BuildState
    {
        Failing,
        Waiting,
        Repository,
        Extracting,
        Building,
    }

    public sealed class BuildPaths
    {
        public string BasePath { get; } = string.Empty;
         
        public string RepoFile { get; } = string.Empty;

        public string RepoPath { get; } = string.Empty;

        public BuildPaths(string basePath)
        {
            basePath.CreateDirectoryIfNotExis();
            BasePath = basePath;
            RepoFile = Path.Combine(basePath, "Repository.zip");
            RepoPath = Path.Combine(basePath, "Repository");
            RepoPath.CreateDirectoryIfNotExis();
        }

        public BuildPaths()
        {
            
        }
    }

    public sealed class BuildData
    {
        public Reporter? Reporter { get; private set; }

        public AppData AppData { get; private set; } = new AppData();
        
        public RepositoryApi Api { get; private set; } = RepositoryApi.Empty;

        public IActorRef CurrentListner { get; private set; } = ActorRefs.Nobody;

        public string Error { get; private set; } = string.Empty;

        public string OperationId { get; private set; } = string.Empty;

        public BuildPaths Paths { get; private set; } = new BuildPaths();

        public BuildData Set(BuildRequest request)
        {
            OperationId = Guid.NewGuid().ToString("D");
            Reporter = request.Source;
            AppData = request.AppData;
            Api = request.RepositoryApi;
            return this;
        }

        public BuildData Set(IncomingDataTransfer request)
        {
            Paths = new BuildPaths(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tauron", "DeploymentServer", OperationId));

            request.Accept(() => File.Create(Paths.RepoFile));

            return this;
        }

        public BuildData SetError(string error)
        {
            Error = error;
            return this;
        }

        public BuildData SetListner(IActorRef list)
        {
            if(!CurrentListner.IsNobody())
                ExposedReceiveActor.ExposedContext.Stop(CurrentListner);

            Paths.BasePath.DeleteDirectory(true);
            CurrentListner = list;

            return this;
        }

        public BuildData Clear()
        {
            ExposedReceiveActor.ExposedContext.Stop(CurrentListner);
            
            return new BuildData();
        }
    }

    public sealed class BuildingActor : FSM<BuildState, BuildData>
    {
        public BuildingActor(IActorRef fileHandler)
        {
            fileHandler.SubscribeToEvent<TransferCompled>();
            fileHandler.SubscribeToEvent<TransferFailed>();

            StartWith(BuildState.Waiting, new BuildData());

            When(BuildState.Waiting, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case TransferFailed _: return Stay();
                    case TransferCompled _: return Stay();
                    case BuildRequest request:
                    {
                        var newData = evt.StateData.Set(request);

                        var listner = Reporter.CreateListner(Context, newData.Reporter!, TimeSpan.FromMinutes(5), task => task.PipeTo(Self));

                        newData.Api.SendAction(new TransferRepository(newData.AppData.Repository, listner, fileHandler, newData.OperationId));
                        return GoTo(BuildState.Repository)
                            .Using(newData.SetListner(listner));
                    }
                    default:
                        return null;
                }
            });

            When(BuildState.Repository, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case OperationResult result:
                        return result.Ok 
                            ? Stay() 
                            : GoTo(BuildState.Failing)
                                .Using(evt.StateData.SetError(result.Error ?? BuildErrorCodes.GernalBuildError));
                    case IncomingDataTransfer transfer:
                        return transfer.OperationId != evt.StateData.OperationId 
                            ? Stay() 
                            : Stay().Using(evt.StateData.Set(transfer));
                    case TransferFailed fail:
                        if (fail.OperationId != evt.StateData.OperationId)
                            return Stay();
                        return GoTo(BuildState.Failing)
                            .Using(evt.StateData.SetError(fail.Reason.ToString()));
                    case TransferCompled c:
                        if (c.OperationId != evt.StateData.OperationId)
                            return Stay();
                        Self.Tell(Trigger.Inst);
                        return GoTo(BuildState.Extracting);
                    default:
                        return null;
                }
            }, TimeSpan.FromMinutes(5));

            When(BuildState.Extracting, evt =>
            {
                switch (evt.FsmEvent)
                {
                    case Trigger _:
                        var paths = evt.StateData.Paths;
                        Task.Run(() =>
                        {
                            ZipFile.ExtractToDirectory(paths.RepoFile, paths.RepoPath, true);
                        }).PipeTo(Self, success:() => new Status.Success(null));
                        return Stay();
                    case Status.Success _:
                        Self.Tell(Trigger.Inst);
                        return GoTo(BuildState.Building);
                    default:
                        return null;
                }
            });

            When(BuildState.Building, evt =>
            {

            });

            When(BuildState.Failing, evt =>
            {
                Context.Parent.Tell(BuildCompled.Inst);
                return GoTo(BuildState.Waiting).Using(evt.StateData.Clear());
            });

            WhenUnhandled(evt =>
            {
                switch (evt.FsmEvent)
                {
                    case IncomingDataTransfer _:
                    case TransferFailed _:
                    case TransferCompled _:
                    case OperationResult _:
                        return Stay();
                    case StateTimeout _ when StateName != BuildState.Waiting:
                        return GoTo(BuildState.Failing).Using(evt.StateData.SetError(Reporter.TimeoutError));
                    case Status.Failure f when StateName != BuildState.Waiting:
                        return GoTo(BuildState.Failing).Using(evt.StateData.SetError(f.Cause.Message));
                    default:
                        return Stay();
                }
            });

            OnTransition((state, nextState) =>
            {
                switch (nextState)
                {
                    case BuildState.Failing:
                        StateData.Reporter?.Compled(OperationResult.Failure(BuildErrorCodes.GernalBuildError));
                        Self.Tell(Trigger.Inst);
                        break;
                }
            });

            Initialize();
        }

        private sealed class Trigger
        {
            public static readonly Trigger Inst = new Trigger();
        }
    }
}