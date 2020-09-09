using System;
using System.IO;
using LibGit2Sharp;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class GitUpdater : IDisposable
    {
        private readonly string _sourceDic;
        private Repository? _repository;

        public GitUpdater(string sourceDic)
        {
            _sourceDic = sourceDic;
            if (Directory.Exists(sourceDic))
            {
                var path = Repository.Discover(sourceDic);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    _repository = new Repository(path);
                    NeedDownload = false;
                    return;
                }
            }

            NeedDownload = true;
        }

        public bool NeedDownload { get; }

        public string Update()
        {
            if(_repository == null)
                throw new InvalidOperationException("Not Repository Set");

            Commands.Pull(_repository, new Signature("ServiceManager", "Service@Manager.com", DateTimeOffset.Now), new PullOptions());
            return _repository.Info.Path;
        }

        public string Download()
        {
            const string url = "https://github.com/Tauron1990/Project-Manager-Akka.git";

            return Repository.Clone(url, _sourceDic);
        }

        public void Dispose() 
            => _repository?.Dispose();
    }
}