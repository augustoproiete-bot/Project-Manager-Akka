using System;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempDic : ITempInfo, IDisposable
    {

        bool KeepAlive { get; set; }

        ITempDic CreateDic(string name);

        ITempFile CreateFile(string name);

        ITempDic CreateDic();

        ITempFile CreateFile();

    }
}