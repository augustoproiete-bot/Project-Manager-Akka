using System;
using LibGit2Sharp;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class GitUpdater : IDisposable
    {
        private Repository? _repository;

        public GitUpdater(string sourceDic)
        {
            
        }

        public bool NeedDownload { get; }

        public void Update()
        {

        }

        public void Download()
        {

        }

        public void Dispose() 
            => _repository?.Dispose();
    }
}