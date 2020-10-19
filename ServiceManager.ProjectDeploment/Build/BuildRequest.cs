using System.Threading.Tasks;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.FileTransfer.TemporarySource;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Build
{
    public sealed class BuildRequest
    {
        public Reporter Source { get; }

        public AppData AppData { get; }
        public RepositoryApi RepositoryApi { get; }

        public TempFile TargetFile { get; }

        public TaskCompletionSource<(string, TempStream)> CompletionSource { get; }

        private BuildRequest(Reporter source, AppData appData, RepositoryApi repositoryApi, TempFile targetFile)
        {
            CompletionSource = new TaskCompletionSource<(string, TempStream)>();
            Source = source;
            AppData = appData;
            RepositoryApi = repositoryApi;
            TargetFile = targetFile;
        }

        public static Task<(string, TempStream)> SendWork(WorkDistributor<BuildRequest, BuildCompled> distributor, Reporter source, AppData appData, RepositoryApi repositoryApi, TempFile targetFile)
        {
            var request = new BuildRequest(source, appData, repositoryApi, targetFile);
            distributor.PushWork(request);
            return request.CompletionSource.Task;
        }
    }
}