using System;
using System.Collections.Generic;
using System.Windows.Markup;
using Akka.Util.Internal;
using JetBrains.Annotations;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Wpf.UI
{
    [MarkupExtensionReturnType(typeof(object))]
    [PublicAPI]
    public sealed class Loc : UpdatableMarkupExtension
    {
        private static Dictionary<string, object?> _cache = new Dictionary<string, object?>();

        public Loc(string entryName) => EntryName = entryName;

        public string EntryName { get; set; }

        protected override object DesignTime()
        {
            if (EntryName?.Length > 25)
                return EntryName.Substring(EntryName.Length - 25, 10);
            return EntryName ?? nameof(DesignTime);
        }

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            try
            {
                lock(_cache)
                    if (_cache.TryGetValue(EntryName, out var result)) return result!;
                
                ActorApplication.Application.ActorSystem.Loc().Request(EntryName, o =>
                {
                    var res = o ?? EntryName;
                    lock(_cache)
                        _cache[EntryName] = res;
                    UpdateValue(res);
                });

                return "Loading";
            }
            catch (Exception)
            {
                return "Error...";
            }
        }
    }
}