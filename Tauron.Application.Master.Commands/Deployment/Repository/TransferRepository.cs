using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed class TransferRepository : FileTransferCommand<RepositoryApi, TransferRepository>, IRepositoryAction
    {
        public string RepoName { get; }
        public string OperationId { get; }
        
        protected override string Info => RepoName;

        public TransferRepository(string repoName, string operationId)
        {
            RepoName = repoName;
            OperationId = operationId;
        }
    }
}