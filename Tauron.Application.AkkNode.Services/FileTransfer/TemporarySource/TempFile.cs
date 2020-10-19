using System;
using System.IO;
using Akka.Util.Internal;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.FileTransfer.TemporarySource
{
    [PublicAPI]
    public sealed class TempFile : IDisposable
    {
        private readonly AtomicCounter _referenceCounter = new AtomicCounter();

        private Stream? _tempFile;
        private bool _isDisposed;
        private string? _targetFile;

        public string TargetFile => _targetFile ??= Path.GetTempFileName();

        public TempFile(string targetFile, bool noCheck = false)
        {
            if(noCheck)
                _referenceCounter.GetAndSet(int.MinValue);
            _targetFile = targetFile;
        }

        public TempFile(bool noCheck = false)
        {
            if (noCheck)
                _referenceCounter.GetAndSet(int.MinValue);
        }

        public TempData CreateDate() 
            => new TempData(this);

        public TempStream CreateStream()
            => new TempStream(this);

        internal Stream GetStream()
        {
            if(_isDisposed)
                throw new ObjectDisposedException(nameof(TargetFile));

            return _tempFile ??= new FileStream(TargetFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete | FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose);
        }

        internal void AddUsage() => _referenceCounter.Next();

        internal void RemoveUage() => _referenceCounter.Decrement();

        internal void TryDispose()
        {
            if (_referenceCounter.Current > 0)
                return;

            ActualDipose();
        }

        public void Dispose()
        {
            if(_referenceCounter.Current > 0)
                throw new TempFileInUseException("The file is in use");

            ActualDipose();
        }

        public void ForceDispose()
            => ActualDipose();

        private void ActualDipose()
        {
            lock (this)
            {
                if(_isDisposed) return;
                _isDisposed = true;
                _tempFile?.Dispose();
                try
                {
                    TargetFile.DeleteDirectory();
                }
                catch (IOException)
                { }
            }
        }
    }
}