using System;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Ionic.Zip;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Octokit;
using ServiceManager.ProjectRepository.Data;
using Tauron;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor
    {
        private readonly IMongoCollection<RepositoryEntry> _repos;
        private readonly GridFSBucket _bucket;
        private readonly GitHubClient _gitHubClient;

        public OperatorActor(IMongoCollection<RepositoryEntry> repos, GridFSBucket bucket)
        {
            _repos = repos;
            _bucket = bucket;
            _gitHubClient = new GitHubClient(new ProductHeaderValue(Context.System.Settings.Config.GetString("akka.appinfo.applicationName")));

            Receive<RegisterRepository>(r => TryExecute(r, "RegisterRepository", RegisterRepository));
            Receive<TransferRepository>(r => TryExecute(r, "RequestRepository", RequestRepository));
        }

        private void RegisterRepository(RegisterRepository repository, Reporter reporter)
        {
            reporter.Send(RepositoryMessages.GetRepo);
            var data = _repos.AsQueryable().FirstOrDefault(e => e.RepoName == repository.RepoName);

            if (data == null)
            {
                reporter.Compled(OperationResult.Failure(ErrorCodes.DuplicateRepository));
                return;
            }

            if (!repository.RepoName.Contains('/'))
            {
                reporter.Compled(OperationResult.Failure(ErrorCodes.InvalidRepoName));
                return;
            }
            var nameSplit = repository.RepoName.Split('/');
            var repoInfo = _gitHubClient.Repository.Get(nameSplit[0], nameSplit[1]).Result;

            if (repoInfo == null)
            {
                reporter.Compled(OperationResult.Failure(ErrorCodes.GithubNoRepoFound));
                return;
            }


            data = new RepositoryEntry
                   {
                        RepoName = repository.RepoName,
                        FileName = repository.RepoName + ".zip",
                        SourceUrl = repoInfo.CloneUrl,
                        RepoId = repoInfo.Id
                   };

            _repos.InsertOne(data);

            reporter.Compled(OperationResult.Success());
        }

        private void RequestRepository(TransferRepository repository, Reporter reporter)
        {
            reporter.Send(RepositoryMessages.GetRepo);

            var data = _repos.AsQueryable().FirstOrDefault(r => r.RepoName == repository.RepoName);
            if (data == null)
            {
                reporter.Compled(OperationResult.Failure(ErrorCodes.DatabaseNoRepoFound));
                return;
            }

            var commitInfo = _gitHubClient.Repository.Commit.Get(data.RepoId, "HEAD").Result;

            if (commitInfo.Commit.Tree.Sha != data.LastUpdate)
            {
                if (string.IsNullOrWhiteSpace(data.FileName))
                {

                }
                else
                {
                    
                }
            }
        }

        protected override void TryExecute<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
        {
            try
            {
                base.TryExecute(msg, name, process);
            }
            finally
            {
                Context.Stop(Self);
            }
        }
    }
}