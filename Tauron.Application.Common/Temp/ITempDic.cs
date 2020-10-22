using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempDic : ITempInfo
    {
        bool KeepAlive { get; set; }

        ITempDic CreateDic(string name);

        ITempFile CreateFile(string name);

        ITempDic CreateDic();

        ITempFile CreateFile();

        void Clear();
    }
}