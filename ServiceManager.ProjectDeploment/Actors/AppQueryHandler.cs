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
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppQueryHandler : ReportingActor 
    {
        public AppQueryHandler(IMongoCollection<AppData> apps, GridFSBucket files, IActorRef dataTransfer)
        {
            MakeQueryCall<QueryApps, AppList>("QueryApps", (query, _) 
                    => new AppList(apps.AsQueryable().Select(ad => new AppInfo(ad.Name, ad.Last.Version, ad.LastUpdate, ad.CreationTime, ad.Repository))));

            MakeQueryCall<QueryApp, AppInfo>("QueryApp", (query, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(e => e.Name == query.AppName);

                if (data != null) return new AppInfo(data.Name, data.Last.Version, data.LastUpdate, data.CreationTime, data.Repository);
                
                reporter.Compled(OperationResult.Failure(ErrorCodes.QueryAppNotFound));
                return null;
            });

            MakeQueryCall<QueryBinarys, FileTransactionId>("QueryBinaries", (query, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(e => e.Name == query.AppName);

                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(ErrorCodes.QueryAppNotFound));
                    return null;
                }

                var targetVersion = query.Version != -1 ? query.Version : data.Last.Version;

                var file = data.Versions.FirstOrDefault(f => f.Version.Version == targetVersion);
                if (file == null)
                {
                    reporter.Compled(OperationResult.Failure(ErrorCodes.QueryFileNotFound));
                    return null;
                }

                var request = DataTransferRequest.FromStream(() => files.OpenDownloadStream(file.File), query.DataManager, query.AppName);
                dataTransfer.Tell(request);

                return new FileTransactionId(request.OperationId);
            });

            MakeQueryCall<QueryBinaryInfo, BinaryList>("QueryBinaryInfo", (binarys, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(ad => ad.Name == binarys.AppName);
                if (data != null) return new BinaryList(data.Versions.Select(i => new AppBinary(i.Version.Version, i.CreationTime, i.Deleted, i.Commit, data.Repository)));
                
                reporter.Compled(OperationResult.Failure(ErrorCodes.QueryAppNotFound));
                return null;

            });
        }

        private void MakeQueryCall<T, TResult>(string name, Func<T, Reporter, TResult> handler)
            where T : DeploymentQueryBase<TResult>
            where TResult : InternalSerializableBase
        {
            Receive<T>(name, (msg, reporter) =>
            {
                var outcome = handler(msg, reporter);
                reporter.Compled(outcome == default && !reporter.IsCompled ? OperationResult.Failure(ErrorCodes.GeneralQueryFailed) : OperationResult.Success(outcome));
            });
        }
    }
}