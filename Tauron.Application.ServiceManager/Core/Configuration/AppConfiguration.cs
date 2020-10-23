using System.IO;
using ServiceManager.ProjectRepository.Core;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace Tauron.Application.ServiceManager.Core.Configuration
{
    public sealed class AppConfiguration : ISettingProviderConfiguration
    {
        public string Scope => SettingTypes.AppConfig;
        public ISettingProvider Provider => new JsonProvider(Path.Combine(RepoEnv.DataPath, "appconfig.json"));
    }
}