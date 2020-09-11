using System;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class BuildResult : IDisposable
    {
        public string Zip { get; }
        private readonly string _buildRoot;
        private readonly Action _compled;

        public BuildResult(string zip, string buildRoot, Action compled)
        {
            Zip = zip;
            _buildRoot = buildRoot;
            _compled = compled;
        }

        public void Compled() => _compled();

        public void Dispose()
        {
            Zip.DeleteFile();
            _buildRoot.DeleteDirectory();
        }
    }
}