using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public sealed class TempStorage : TempDic
    {
        private static TempStorage? _default;

        public static TempStorage Default => _default ??= new TempStorage();
        
        public TempStorage()
            : this(Path.GetTempFileName, Path.GetTempPath(), false)
        { }

        public TempStorage(Func<string> nameProvider, string basePath, bool deleteBasePath)
            : base(basePath, null, nameProvider, deleteBasePath)
        {
            WireUp();
        }

        private void WireUp() 
            => AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                Dispose();
            }
            catch
            {
                //Ignored Due to Process Exit
            }
        }

        protected override void DisposeCore(bool disposing)
        {
            if (_default == this)
                _default = null;
            base.DisposeCore(disposing);
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        }
    }
}