using System;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempInfo : IDisposable
    {
        string          FullPath { get; }
        Maybe<ITempDic> Parent   { get; }
    }
}