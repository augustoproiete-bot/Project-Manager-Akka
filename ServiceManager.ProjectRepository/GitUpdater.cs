using System;
using System.IO;
using LibGit2Sharp;

namespace ServiceManager.ProjectRepository
{
    public sealed class GitUpdater : SharedObject<GitUpdater>
    { private Repository? _repository;

        private void Init(string sourceDic)
        {
            if(_repository != null)
                return;

            if (!Directory.Exists(sourceDic)) return;
            
            var path = Repository.Discover(sourceDic);

            if (!string.IsNullOrWhiteSpace(path))
            {
                _repository = new Repository(path);
            }
        }

        public string RunUpdate(string tracking, string source)
        {
            lock (Lock)
            {
                Init(source);

                if (_repository == null)
                {
                    LogMessage("Download Repository {Id}", tracking);
                    return Download(source);
                }

                LogMessage("update Repository {Id}", tracking);
                return Update(source);
            }

        }

        private string Update(string source)
        {
            if(_repository == null)
                throw new InvalidOperationException("Not Repository Set");

            Commands.Pull(_repository, new Signature("ServiceManager", "Service@Manager.com", DateTimeOffset.Now), new PullOptions()); 
            return _repository.Info.Path;
        }

        private string Download(string source)
        {
            _repository =new Repository(Repository.Clone(Configuration.CloneUrl, source));
            return _repository.Info.Path;
        }

        protected override void InternalDispose()
        {
            _repository?.Dispose();
            base.InternalDispose();
        }
    }
}