using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Repository
{
    public sealed class RegisterRepository : SimpleCommand<RepositoryApi, RegisterRepository>, IRepositoryAction
    {
        public string RepoName { get; }

        public bool IgnoreDuplicate { get; set; }

        public RegisterRepository(string repoName) 
            => RepoName = repoName;

        protected override string Info => RepoName;
    }
}