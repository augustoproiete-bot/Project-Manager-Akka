using System;
using System.Linq;
using Akka.Actor;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Deployment;
using Tauron.Application.Master.Commands.Deployment.Deployment.Data;
using Tauron.Application.Master.Commands.Deployment.Deployment.Querys;

namespace ServiceManager.ProjectDeployment.Actors
{
    public sealed class AppQueryHandler : ExposedReceiveActor
    {
        public AppQueryHandler(IMongoCollection<AppData> apps, GridFSBucket files, IActorRef dataTransfer)
        {
            MakeQueryCall<QueryApps, AppList>((query, _) 
                    => new AppList(apps.AsQueryable().Select(ad => new AppInfo(ad.Name, ad.Last.Version, ad.LastUpdate, ad.CreationTime))));

            MakeQueryCall<QueryApp, AppInfo>((query, reporter) =>
            {
                var data = apps.AsQueryable().FirstOrDefault(e => e.Name == query.AppName);

                if (data != null) return new AppInfo(data.Name, data.Last.Version, data.LastUpdate, data.CreationTime);
                
                reporter.Compled(OperationResult.Failure(ErrorCodes.QueryAppNotFound));
                return null;
            });

            MakeQueryCall<QueryBinarys, FileTransactionId>((query, reporter) =>
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
        }

        private void MakeQueryCall<T, TResult>(Func<T, Reporter, TResult> handler)
            where T : DeploymentQueryBase<TResult>
            where TResult : InternalSerializableBase
        {
            Receive<T>(q =>
            {
                var reporter = Reporter.CreateReporter(Context);
                reporter.Listen(q.Listner);

                try
                {
                    var outcome = handler(q, reporter);
                    reporter.Compled(outcome == default && !reporter.IsCompled ? OperationResult.Failure(ErrorCodes.GeneralQueryFailed) : OperationResult.Success(outcome));
                }
                catch (Exception e)
                {
                    reporter.Compled(OperationResult.Failure(e.Message));
                }
            });
        }
    }
}