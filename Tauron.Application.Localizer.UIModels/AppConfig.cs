using System;
using System.Collections.Immutable;
using Tauron.Akka;
using Tauron.Application.Settings;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class AppConfig : ConfigurationBase
    {
        private ImmutableList<string>? _renctFiles;

        public AppConfig(IDefaultActorRef<SettingsManager> actor, string scope)
            : base(actor, scope)
        {
        }

        public ImmutableList<string> RenctFiles
        {
            get { return _renctFiles ??= ImmutableList<string>.Empty.AddRange(GetValue(s => s.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries), Array.Empty<string>())); }
            set
            {
                _renctFiles = value;
                SetValue(string.Join(';', _renctFiles));
            }
        }
    }
}