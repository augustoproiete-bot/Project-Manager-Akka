using System.IO;
using Functional.Maybe;

namespace Tauron.Temp
{
    public sealed class TempFile : DisposeableBase, ITempFile
    {
        private Stream? _targetStream;

        public TempFile(string targetPath, Maybe<ITempDic> parent)
        {
            Parent   = parent;
            FullPath = targetPath;
        }

        internal Stream InternalStrem => _targetStream ??= new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete | FileShare.Read, 4096, FileOptions.DeleteOnClose);

        public Maybe<ITempDic> Parent { get; }

        public bool NoStreamDispose { get; set; }

        public Stream Stream => new TempStream(this);

        public string FullPath { get; }

        protected override void DisposeCore(bool disposing)
            => _targetStream?.Dispose();
    }
}