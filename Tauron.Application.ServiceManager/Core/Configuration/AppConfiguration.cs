using System;
using System.IO;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace Tauron.Application.ServiceManager.Core.Configuration
{
    public sealed class AppConfiguration : ISettingProviderConfiguration
    {
        public string Scope => SettingTypes.AppConfig;
        public ISettingProvider Provider => new JsonProvider(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron\\ServiceManager", "appconfig.json"));
    }
}