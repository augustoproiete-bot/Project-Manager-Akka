using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Akka.Actor;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Octokit;
using ServiceManager.ProjectRepository.Core;
using ServiceManager.ProjectRepository.Data;
using Tauron;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.CleanUp;
using Tauron.Application.AkkNode.Services.Commands;
using Tauron.Application.AkkNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Operations;
using Tauron.Temp;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReportingActor, IWithTimers
    {
        private static readonly ReaderWriterLockSlim UpdateLock = new ReaderWriterLockSlim();

        private readonly IMongoCollection<RepositoryEntry> _repos;
        private readonly GridFSBucket _bucket;
        private readonly IMongoCollection<ToDeleteRevision> _revisions;
        private readonly DataTransferManager _dataTransfer;
        private readonly GitHubClient _gitHubClient;
        private readonly Dictionary<string, ITempFile> _currentTransfers = new Dictionary<string, ITempFile>();

        public OperatorActor(IMongoCollection<RepositoryEntry> repos, GridFSBucket bucket, IMongoCollection<ToDeleteRevision> revisions, DataTransferManager dataTransfer)
        {
            _repos = repos;
            _bucket = bucket;
            _revisions = revisions;
            _dataTransfer = dataTransfer;
            _gitHubClient = new GitHubClient(new ProductHeaderValue(Context.System.Settings.Config.GetString("akka.appinfo.applicationName", "Test Apps").Replace(' ', '_')));

            Receive<RegisterRepository>(r =>
            {
                TryExecute(r, "RegisterRepository", RegisterRepository);
                Context.Stop(Self);
            });
            
            Receive<TransferRepository>(r => TryExecute(r, "RequestRepository", RequestRepository));

            Receive<TransferMessages.TransferCompled>(c =>
            {
                if(_currentTransfers.TryGetValue(c.OperationId, out var f))
                    f.Dispose();
            });
        }

        private void RegisterRepository(RegisterRepository repository, Reporter reporter)
        {
            UpdateLock.EnterUpgradeableReadLock();
            try
            {
                Log.Info("Incomming Registration Request for Repository {Name}", repository.RepoName);

                reporter.Send(RepositoryMessages.GetRepo);
                var data = _repos.AsQueryable().FirstOrDefault(e => e.RepoName == repository.RepoName);

                if (data != null)
                {
                    Log.Info("Repository {Name} is Registrated", repository.RepoName);
                    if (repository.IgnoreDuplicate)
                    {
                        reporter.Compled(OperationResult.Success());
                        return;
                    }
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.DuplicateRepository));
                    return;
                }

                if (!repository.RepoName.Contains('/'))
                {
                    Log.Info("Repository {Name} Name is Invalid", repository.RepoName);
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.InvalidRepoName));
                    return;
                }

                var nameSplit = repository.RepoName.Split('/');
                var repoInfo = _gitHubClient.Repository.Get(nameSplit[0], nameSplit[1]).Result;

                if (repoInfo == null)
                {
                    Log.Info("Repository {Name} Name not found on Github", repository.RepoName);
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.GithubNoRepoFound));
                    return;
                }

                Log.Info("Savin new Repository {Name} on Database", repository.RepoName);
                data = new RepositoryEntry
                       {
                           RepoName = repository.RepoName,
                           SourceUrl = repoInfo.CloneUrl,
                           RepoId = repoInfo.Id
                       };

                UpdateLock.EnterWriteLock();
                try
                {
                    _repos.InsertOne(data);
                }
                finally
                {
                    UpdateLock.ExitWriteLock();
                }

                reporter.Compled(OperationResult.Success());
            }
            finally
            {
                UpdateLock.ExitUpgradeableReadLock();
            }
        }

        private void RequestRepository(TransferRepository repository, Reporter reporter)
        {
            var repozipFile = RepoEnv.TempFiles.CreateFile();
            UpdateLock.EnterUpgradeableReadLock();
            try
            {
                Log.Info("Incomming Transfer Request for Repository {Name}", repository.RepoName);
                reporter.Send(RepositoryMessages.GetRepo);

                var data = _repos.AsQueryable().FirstOrDefault(r => r.RepoName == repository.RepoName);
                if (data == null)
                {
                    reporter.Compled(OperationResult.Failure(RepoErrorCodes.DatabaseNoRepoFound));
                    return;
                }

                var commitInfo = _gitHubClient.Repository.Commit.GetSha1(data.RepoId, "HEAD").Result;

                var repozip = repozipFile.Stream;

                if (!(commitInfo != data.LastUpdate && UpdateRepository(data, reporter, repository, commitInfo, repozip)))
                {
                    reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                    Log.Info("Downloading Repository {Name} From Server", repository.RepoName);
                    repozip.SetLength(0);
                    _bucket.DownloadToStream(ObjectId.Parse(data.FileName), repozip);
                }

                //_reporter = reporter;

                //repozip.Seek(0, SeekOrigin.Begin);
                //Timers.StartSingleTimer(_reporter, new TransferFailed(string.Empty, FailReason.Timeout, data.RepoName), TimeSpan.FromMinutes(10));
                var request = DataTransferRequest.FromStream(repository.OperationId, repozip, repository.Manager ?? throw new ArgumentNullException("FileManager"), commitInfo);
                request.SendCompletionBack = true;

                _dataTransfer.Request(request);
                _currentTransfers[request.OperationId] = repozipFile;

                reporter.Compled(OperationResult.Success(new FileTransactionId(request.OperationId)));
            }
            finally
            {
                UpdateLock.ExitUpgradeableReadLock();
            }
        }
        
        private bool UpdateRepository(RepositoryEntry data, Reporter reporter, TransferRepository repository, string commitInfo, Stream repozip)
        {
            Log.Info("Try Update Repository");
            UpdateLock.EnterWriteLock();
            try
            {
                var downloadCompled = false;
                var repoConfiguration = new RepositoryConfiguration(reporter, data);
                using var repoPath = RepoEnv.TempFiles.CreateDic();

                var data2 = _repos.AsQueryable().FirstOrDefault(r => r.RepoName == repository.RepoName);
                if (data2 != null && commitInfo != data2.LastUpdate)
                {
                    if (!string.IsNullOrWhiteSpace(data.FileName))
                    {
                        try
                        {
                            Log.Info("Downloading Repository {Name} From Server", repository.RepoName);

                            reporter.Send(RepositoryMessages.GetRepositoryFromDatabase);
                            _bucket.DownloadToStream(ObjectId.Parse(data.FileName), repozip);

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
                        unpackZip.ExtractToDirectory(repoPath.FullPath);
                    }

                    Log.Info("Execute Git Pull for {Name}", repository.RepoName);
                    using var updater = GitUpdater.GetOrNew(repoConfiguration);
                    var result = updater.RunUpdate(repoPath.FullPath);
                    var dataUpdate = Builders<RepositoryEntry>.Update.Set(e => e.LastUpdate, result.Sha);

                    Log.Info("Compress Repository {Name}", repository.RepoName);
                    reporter.Send(RepositoryMessages.CompressRepository);

                    if (repozip.Length != 0)
                        repozip.SetLength(0);

                    using (var archive = new ZipArchive(repozip, ZipArchiveMode.Create, true))
                        archive.AddFilesFromDictionary(repoPath.FullPath);

                    repozip.Seek(0, SeekOrigin.Begin);

                    Log.Info("Upload and Update Repository {Name}", repository.RepoName);
                    reporter.Send(RepositoryMessages.UploadRepositoryToDatabase);
                    var current = data.FileName;
                    var id = _bucket.UploadFromStream(repository.RepoName.Replace('/', '_') + ".zip", repozip);
                    dataUpdate = dataUpdate.Set(e => e.FileName, id.ToString());

                    if (!string.IsNullOrWhiteSpace(current))
                        _revisions.InsertOne(new ToDeleteRevision(current));

                    _repos.UpdateOne(e => e.RepoName == data.RepoName, dataUpdate);
                    data.FileName = id.ToString();

                    repozip.Seek(0, SeekOrigin.Begin);

                    return true;
                }
            }
            finally
            {
                UpdateLock.ExitWriteLock();
            }

            return false;
        }

        public ITimerScheduler Timers { get; set; } = null!;
    }
}