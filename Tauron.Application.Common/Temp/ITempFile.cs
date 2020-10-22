using System;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempFile : ITempInfo
    {
        Stream Stream { get; }
    }
}