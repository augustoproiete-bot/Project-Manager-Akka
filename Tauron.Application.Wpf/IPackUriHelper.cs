using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public interface IPackUriHelper
    {
        string GetString(string pack);

        string GetString(string pack, string? assembly, bool full);

        Uri GetUri(string pack);

        Uri GetUri(string pack, string? assembly, bool full);

        [MethodImpl(MethodImplOptions.NoInlining)]
        T Load<T>(string pack) where T : class;

        [MethodImpl(MethodImplOptions.NoInlining)]
        T Load<T>(string pack, string? assembly) where T : class;

        [MethodImpl(MethodImplOptions.NoInlining)]
        Stream LoadStream(string pack);

        [MethodImpl(MethodImplOptions.NoInlining)]
        Stream LoadStream(string pack, string? assembly);
    }
}