using System;
using System.IO;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public sealed class TempStorage : IDisposable
    {
        private static TempStorage? _default;

        public static TempStorage Default => _default ??= new TempStorage();

        public Func<string> NameProvider { get; }

        public string BasePath { get; }

        public bool DeleteBasePath { get; }


        public TempStorage()
            : this(Path.GetTempFileName, Path.GetTempPath(), false)
        { }

        public TempStorage(Func<string> nameProvider, string basePath, bool deleteBasePath)
        {
            NameProvider = nameProvider;
            BasePath = basePath;
            DeleteBasePath = deleteBasePath;

            WireUp();
        }

        private void WireUp() 
            => AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        private void OnProcessExit(object sender, EventArgs e)
        {
            
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
            if (_default == this)
                Interlocked.Exchange(ref _default, null!);
        }
    }
}