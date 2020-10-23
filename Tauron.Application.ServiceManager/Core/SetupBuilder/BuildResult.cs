using System;
using Tauron.Temp;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class BuildResult : IDisposable
    {
        public ITempFile Zip { get; }
        private readonly Action _compled;

        public BuildResult(ITempFile zip, Action compled)
        {
            Zip = zip;
            _compled = compled;
        }

        public void Compled() => _compled();

        public void Dispose() => Zip.Dispose();
    }
}