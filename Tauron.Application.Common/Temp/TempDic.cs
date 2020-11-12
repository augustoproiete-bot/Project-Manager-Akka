using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Serilog;

namespace Tauron.Temp
{
    public class TempDic : DisposeableBase, ITempDic
    {
        public static readonly ITempDic Null = new TempDic();

        private readonly Func<string> _nameGenerator;
        private readonly bool _deleteDic;
        private readonly ConcurrentDictionary<string, ITempDic> _tempDics = new();
        private readonly ConcurrentDictionary<string, ITempFile> _tempFiles = new();

        public string FullPath { get; }
        public ITempDic? Parent { get; }
        public bool KeepAlive { get; set; }

        protected TempDic(string fullPath, ITempDic? parent, Func<string> nameGenerator, bool deleteDic)
        {
            _nameGenerator = nameGenerator;
            _deleteDic = deleteDic;
            FullPath = fullPath;
            Parent = parent;

            fullPath.CreateDirectoryIfNotExis();
        }

        private TempDic()
        {
            FullPath = string.Empty;
            KeepAlive = true;
            _deleteDic = false;
            _nameGenerator = () => string.Empty;
        }

        private void CheckNull()
        {
            if(string.IsNullOrEmpty(FullPath))
                throw new NotSupportedException("The Path is Empty");
        }

        public ITempDic CreateDic(string name)
        {
            CheckNull();
            return _tempDics.GetOrAdd(name, s =>
            {
                var dic = new TempDic(Path.Combine(FullPath, s), this, _nameGenerator, true);
                dic.TrackDispose(() => _tempDics.TryRemove(s, out _));
                return dic;
            });
        }

        public ITempFile CreateFile(string name)
        {
            CheckNull();
            return _tempFiles.GetOrAdd(name, s =>
            {
                var file = new TempFile(Path.Combine(FullPath, s), this);
                file.TrackDispose(() => _tempFiles.TryRemove(s, out _));
                return file;
            });
        }

        public ITempDic CreateDic() => CreateDic(_nameGenerator());

        public ITempFile CreateFile() => CreateFile(_nameGenerator());
        public void Clear()
        {
            if (string.IsNullOrWhiteSpace(FullPath))
                return;

            void TryDispose(IEnumerable<ITempInfo> toDispose)
            {
                foreach (var entry in toDispose)
                {
                    try
                    {
                        entry.Dispose();
                    }
                    catch (Exception e)
                    {
                        if (KeepAlive)
                            Log.ForContext(GetType()).Warning(e, "Error on Dispose Dic {Path}", entry.FullPath);
                        else
                            throw;
                    }
                }
            }

            try
            {
                var dics = _tempDics.Values;
                dics.Foreach(t => t.KeepAlive = KeepAlive);

                TryDispose(dics);
                TryDispose(_tempFiles.Values);
            }
            finally
            {
                _tempDics.Clear();
                _tempFiles.Clear();
            }
        }

        protected override void DisposeCore(bool disposing)
        {
            if (!disposing) return;

            Clear();

            if (_deleteDic)
                FullPath.DeleteDirectory(true);
        }
    }
}