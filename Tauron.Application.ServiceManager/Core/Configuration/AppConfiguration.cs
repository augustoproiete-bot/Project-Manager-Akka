using System.IO;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace Tauron.Application.ServiceManager.Core.Configuration
{
    public sealed class AppConfiguration : ISettingProviderConfiguration
    {
        public string Scope => SettingTypes.AppConfig;
        public ISettingProvider Provider => new JsonProvider(Path.Combine(Env.Path, "appconfig.json"));
    }
}