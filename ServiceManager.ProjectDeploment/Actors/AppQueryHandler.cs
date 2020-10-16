using System;
using System.Linq;
using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppQueryHandler : ReportingActor 
    {
        public AppQueryHandler(IMongoCollection<AppData> apps, GridFSBucket files, IActorRef dataTransfer, IActorRef changeTracker)
        {
            Receive<QueryChangeSource>(changeTracker.Forward);

            MakeQueryCall<QueryApps, AppList>("QueryApps", (query, _) 
                    => new AppList(apps.AsQueryable().Select(ad => ad.ToInfo())));

            MakeQueryCall<QueryApp, AppInfo>("QueryApp", (query, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(e => e.Name == query.AppName);

                if (data != null) return data.ToInfo();
                
                reporter.Compled(OperationResult.Failure(BuildErrorCodes.QueryAppNotFound));
                return null;
            });

            MakeQueryCall<QueryBinarys, FileTransactionId>("QueryBinaries", (query, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(e => e.Name == query.AppName);

                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.QueryAppNotFound));
                    return null;
                }

                var targetVersion = query.AppVersion != -1 ? query.AppVersion : data.Last;

                var file = data.Versions.FirstOrDefault(f => f.Version == targetVersion);
                if (file == null)
                {
                    reporter.Compled(OperationResult.Failure(BuildErrorCodes.QueryFileNotFound));
                    return null;
                }

                var request = DataTransferRequest.FromStream(() => files.OpenDownloadStream(file.File), query.DataManager, query.AppName);
                dataTransfer.Tell(request);

                return new FileTransactionId(request.OperationId);
            });

            MakeQueryCall<QueryBinaryInfo, BinaryList>("QueryBinaryInfo", (binarys, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(ad => ad.Name == binarys.AppName);
                if (data != null) return new BinaryList(data.Versions.Select(i => new AppBinary(i.Version, i.CreationTime, i.Deleted, i.Commit, data.Repository)));
                
                reporter.Compled(OperationResult.Failure(BuildErrorCodes.QueryAppNotFound));
                return null;

            });
        }

        private void MakeQueryCall<T, TResult>(string name, Func<T, Reporter, TResult?> handler)
            where T : DeploymentQueryBase<TResult>
            where TResult : InternalSerializableBase
        {
            Receive<T>(name, (msg, reporter) =>
            {
                var outcome = handler(msg, reporter);
                reporter.Compled(outcome == default && !reporter.IsCompled ? OperationResult.Failure(BuildErrorCodes.GeneralQueryFailed) : OperationResult.Success(outcome));
            });
        }
    }
}