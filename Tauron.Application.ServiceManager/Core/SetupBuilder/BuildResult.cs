using System;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class BuildResult : IDisposable
    {
        public string Zip { get; }
        private readonly string _buildRoot;

        public BuildResult(string zip, string buildRoot)
        {
            Zip = zip;
            _buildRoot = buildRoot;
        }


        public void Dispose()
        {
            Zip.DeleteFile();
            _buildRoot.DeleteDirectory();
        }
    }
}