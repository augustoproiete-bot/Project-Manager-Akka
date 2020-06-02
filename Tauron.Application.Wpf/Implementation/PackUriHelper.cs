using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Implementation
{
    [PublicAPI]
    public class PackUriHelper : IPackUriHelper
    {
        public string GetString(string pack)
        {
            return GetString(pack, Assembly.GetCallingAssembly().GetName().Name, false);
        }

        public string GetString(string pack, string? assembly, bool full)
        {
            if (assembly == null) return pack;

            var fullstring = full ? "pack://application:,,," : string.Empty;
            return $"{fullstring}/{assembly};component/{pack}";
        }

        public Uri GetUri(string pack)
        {
            return GetUri(pack, Assembly.GetCallingAssembly().GetName().Name, false);
        }

        public Uri GetUri(string pack, string? assembly, bool full)
        {
            var compledpack = GetString(pack, assembly, full);
            var uriKind = full ? UriKind.Absolute : UriKind.Relative;

            return new Uri(compledpack, uriKind);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public T Load<T>(string pack) where T : class
        {
            return Load<T>(pack, Assembly.GetCallingAssembly().GetName().Name);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public T Load<T>(string pack, string? assembly) where T : class
        {
            return (T) System.Windows.Application.LoadComponent(GetUri(pack, assembly, false));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Stream LoadStream(string pack)
        {
            return LoadStream(pack, Assembly.GetCallingAssembly().GetName().Name);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Stream LoadStream(string pack, string? assembly)
        {
            var info = System.Windows.Application.GetResourceStream(GetUri(pack, assembly, true));
            if (info != null) return info.Stream;

            throw new InvalidOperationException();
        }
    }
}