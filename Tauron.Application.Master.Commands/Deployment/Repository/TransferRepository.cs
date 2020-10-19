using Akka.Actor;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Tauron.Application.AkkNode.Services.Commands;
using Tauron.Application.AkkNode.Services.FileTransfer;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed class TransferRepository : FileTransferCommand<RepositoryApi, TransferRepository>
    {
        public string RepoName { get; }
        public string OperationId { get; }
        
        protected override string Info => RepoName;


        public TransferRepository(string repoName, string operationId)
        {
            RepoName = repoName;
            OperationId = operationId;
        }

        [JsonConstructor]
        private TransferRepository([NotNull] IActorRef listner, [NotNull] DataTransferManager manager, string repoName, string operationId) : base(listner, manager)
        {
            RepoName = repoName;
            OperationId = operationId;
        }
    }
}