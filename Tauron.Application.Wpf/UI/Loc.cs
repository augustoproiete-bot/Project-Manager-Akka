using System;
using System.Linq;
using System.Windows.Markup;
using JetBrains.Annotations;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Wpf.UI
{
    [MarkupExtensionReturnType(typeof(object)), PublicAPI]
    public sealed class Loc : UpdatableMarkupExtension
    {
        public string EntryName { get; set; }

        public Loc(string entryName) => EntryName = entryName;
        
        protected override object DesignTime() => nameof(DesignTime);

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            try
            {
                ActorApplication.Application.ActorSystem.Loc().Request(EntryName, UpdateValue);

                return "Loading...";
            }
            catch (Exception)
            {
                return "Error...";
            }
        }
    }
}