using System;
using System.Collections.Immutable;
using System.IO;
using Tauron.Akka;
using Tauron.Application.Settings;

namespace Tauron.Application.ServiceManager.Core.Configuration
{
    public sealed class AppConfig : ConfigurationBase
    {
        private ImmutableList<string>? _renctFiles;
        private string? _currentConfig;

        public AppConfig(IDefaultActorRef<SettingsManager> actor)
            : base(actor, SettingTypes.AppConfig)
        {
        }

        public ImmutableList<string> SeedUrls
        {
            get { return _renctFiles ??= ImmutableList<string>.Empty.AddRange(GetValue(s => s.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries), Array.Empty<string>())); }
            set
            {
                if(_renctFiles == value)
                    return;

                _renctFiles = value;
                SetValue(string.Join(';', _renctFiles));
            }
        }

        public string CurrentConfig
        {
            get => _currentConfig ??= GetValue(s => s, File.ReadAllText("seed.conf"))!;
            set
            {
                if(string.Equals(_currentConfig, value, StringComparison.Ordinal))
                    return;

                _currentConfig = value;
                SetValue(value);
            }
        }
    }
}