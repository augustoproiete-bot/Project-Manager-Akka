using System;
using System.IO;
using LibGit2Sharp;
using Tauron;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository.Core
{
    public sealed class GitUpdater : SharedObject<GitUpdater, RepositoryConfiguration>
    {
        private Repository? _repository;

        private void Init(string sourceDic)
        {
            if (_repository != null)
                return;

            if (!Directory.Exists(sourceDic)) return;

            var path = Repository.Discover(sourceDic);

            if (!string.IsNullOrWhiteSpace(path)) 
                _repository = new Repository(path);
        }

        public (string Path, string Sha) RunUpdate(string source)
        {
            lock (Lock)
            {
                Init(source);

                if (_repository == null)
                {
                    SendMessage(RepositoryMessages.DownloadRepository);
                    return Download(source);
                }

                SendMessage(RepositoryMessages.UpdateRepository);
                return Update(source);
            }

        }

        private (string Path, string Sha) Update(string source)
        {
            if (_repository == null)
                throw new InvalidOperationException("Not Repository Set");

            Commands.Pull(_repository, new Signature("ServiceManager", "Service@Manager.com", DateTimeOffset.Now), new PullOptions());
            return (_repository.Info.WorkingDirectory, _repository.Head.Tip.Sha);
        }

        private (string Path, string Sha) Download(string source)
        {
            source.CreateDirectoryIfNotExis();
            _repository = new Repository(Repository.Clone(Configuration.CloneUrl, source));
            return (_repository.Info.WorkingDirectory, _repository.Head.Tip.Sha);
        }

        protected override void InternalDispose()
        {
            _repository?.Dispose();
            base.InternalDispose();
        }
    }
}