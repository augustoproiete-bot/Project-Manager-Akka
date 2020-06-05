using System;
using System.Windows.Markup;
using JetBrains.Annotations;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Wpf.UI
{
    [MarkupExtensionReturnType(typeof(object))]
    [PublicAPI]
    public sealed class Loc : UpdatableMarkupExtension
    {
        public Loc(string entryName)
        {
            EntryName = entryName;
        }

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
                return ActorApplication.Application.ActorSystem.Loc().Request(EntryName)!;

                //Task.Run(() => ActorApplication.Application.ActorSystem.Loc().Request(EntryName, UpdateValue));

                //return "Loading...";
            }
            catch (Exception)
            {
                return "Error...";
            }
        }
    }
}