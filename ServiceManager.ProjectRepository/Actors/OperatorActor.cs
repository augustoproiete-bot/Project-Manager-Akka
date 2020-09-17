using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Octokit;
using ServiceManager.ProjectRepository.Core;
using ServiceManager.ProjectRepository.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor
    {
        private static readonly ReaderWriterLockSlim UpdateLock = new ReaderWriterLockSlim();

        private readonly IMongoCollection<RepositoryEntry> _repos;
        private readonly GridFSBucket _bucket;
        private readonly IMongoCollection<ToDeleteRevision> _revisions;
        private readonly GitHubClient _gitHubClient;

        public OperatorActor(IMongoCollection<RepositoryEntry> repos, GridFSBucket bucket, IMongoCollection<ToDeleteRevision> revisions)
        {
            _repos = repos;
            _bucket = bucket;
            _revisions = revisions;
            _gitHubClient = new GitHubClient(new ProductHeaderValue(Context.System.Settings.Config.GetString("akka.appinfo.applicationName")));

            Receive<RegisterRepository>(r => TryExecute(r, "RegisterRepository", RegisterRepository));
            Receive<TransferRepository>(r => TryExecute(r, "RequestRepository", RequestRepository));
        }

        private void RegisterRepository(RegisterRepository repository, Reporter reporter)
        {
            Log.Info("Incomming Registration Request for Repository {Name}", repository.RepoName);

            reporter.Send(RepositoryMessages.GetRepo);
            var data = _repos.AsQueryable().FirstOrDefault(e => e.RepoName == repository.RepoName);

            if (data != null)
            {
                Log.Info("Repository {Name} is Registrated", repository.RepoName);
                reporter.Compled(OperationResult.Failure(ErrorCodes.DuplicateRepository));
                return;
            }

            if (!repository.RepoName.Contains('/'))
            {
                Log.Info("Repository {Name} Name is Invalid", repository.RepoName);
                reporter.Compled(OperationResult.Failure(ErrorCodes.InvalidRepoName));
                return;
            }
            var nameSplit = repository.RepoName.Split('/');
            var repoInfo = _gitHubClient.Repository.Get(nameSplit[0], nameSplit[1]).Result;

            if (repoInfo == null)
            {
                Log.Info("Repository {Name} Name not found on Github", repository.RepoName);
                reporter.Compled(OperationResult.Failure(ErrorCodes.GithubNoRepoFound));
                return;
            }

            Log.Info("Savin new Repository {Name} on Database", repository.RepoName);
            data = new RepositoryEntry
                   {
                        RepoName = repository.RepoName,
                        SourceUrl = repoInfo.CloneUrl,
                        RepoId = repoInfo.Id
                   };

            _repos.InsertOne(data);

            reporter.Compled(OperationResult.Success());
        }

        private void RequestRepository(TransferRepository repository, Reporter reporter)
        {
            UpdateLock.EnterUpgradeableReadLock();
            try
            {
                Log.Info("Incomming Transfer Request for Repository {Name}", repository.RepoName);
                reporter.Send(RepositoryMessages.GetRepo);

                var data = _repos.AsQueryable().FirstOrDefault(r => r.RepoName == repository.RepoName);
                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(ErrorCodes.DatabaseNoRepoFound));
                    return;
                }

                var commitInfo = _gitHubClient.Repository.Commit.Get(data.RepoId, "HEAD").Result;

                using var repozip = new MemoryStream();

                if (!(commitInfo.Commit.Tree.Sha != data.LastUpdate && UpdateRepository(data, reporter, repository, commitInfo, repozip)))
                {

                }


            }
            finally
            {
                UpdateLock.ExitUpgradeableReadLock();
            }
        }

        private bool UpdateRepository(RepositoryEntry data, Reporter reporter, RepositoryAction repository, GitHubCommit commitInfo, Stream repozip)
        {
            Log.Info("Try Update Repository");
            UpdateLock.EnterWriteLock();
            try
            {
                var downloadCompled = false;
                var repoConfiguration = new RepositoryConfiguration(reporter, data);

                var data2 = _repos.AsQueryable().FirstOrDefault(r => r.RepoName == repository.RepoName);
                if (data2 != null && commitInfo.Commit.Tree.Sha != data2.LastUpdate)
                {
                    if (!string.IsNullOrWhiteSpace(data.FileName))
                    {
                        try
                        {
                            Log.Info("Downloading Repository {Name} From Server", repository.RepoName);

                            reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                            _bucket.DownloadToStreamByName(data.FileName, repozip);

                            downloadCompled = true;
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error on Download Repo File {Name}", data.FileName);
                        }
                    }

                    if (downloadCompled)
                    {
                        Log.Info("Unpack Repository {Name}", repository.RepoName);

                        repozip.Seek(0, SeekOrigin.Begin);
                        using var unpackZip = new ZipArchive(repozip);

                        reporter.Send(RepositoryMessages.ExtractRepository);
                        unpackZip.ExtractToDirectory(repoConfiguration.SourcePath);
                    }

                    Log.Info("Execute Git Pull for {Name}", repository.RepoName);
                    var updater = GitUpdater.GetOrNew(repoConfiguration);

                    var result = updater.RunUpdate(data.SourceUrl);
                    var dataUpdate = Builders<RepositoryEntry>.Update.Set(e => e.LastUpdate, result.Sha);

                    Log.Info("Compress Repository {Name}", repository.RepoName);
                    reporter.Send(RepositoryMessages.CompressRepository);
                    var temp = Path.GetTempFileName();
                    ZipFile.CreateFromDirectory(result.Path, temp);
                    using var packed = File.OpenRead(temp);
                    var fileName = data.FileName + ".zip";

                    Log.Info("Upload and Update Repository {Name}", repository.RepoName);
                    reporter.Send(RepositoryMessages.UploadRepositoryToDatabase);
                    var current = data.FileName;
                    var id = _bucket.UploadFromStream(fileName, packed);
                    dataUpdate = dataUpdate.Set(e => e.FileName, id.ToString());

                    if (!string.IsNullOrWhiteSpace(current))
                        _revisions.InsertOne(new ToDeleteRevision(current));

                    _repos.UpdateOne(e => e.RepoName == data.RepoName, dataUpdate);
                    data.FileName = id.ToString();

                    Log.Info("Copy current data from {Name}", repository.RepoName);
                    packed.Seek(0, SeekOrigin.Begin);
                    repozip.SetLength(0);

                    packed.CopyTo(repozip);

                    return true;
                }
            }
            finally
            {
                UpdateLock.ExitWriteLock();
            }

            return false;
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