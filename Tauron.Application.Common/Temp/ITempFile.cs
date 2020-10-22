using System.IO;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempFile : ITempInfo
    {
        bool NoStreamDispose { get; set; }

        Stream Stream { get; }
    }
}