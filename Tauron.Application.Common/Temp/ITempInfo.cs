using System;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempInfo : IDisposable
    {
        string FullPath { get; }
        ITempDic? Parent { get; }
    }
}