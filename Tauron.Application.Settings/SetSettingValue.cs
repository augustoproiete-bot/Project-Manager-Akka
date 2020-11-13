

namespace Tauron.Application.Settings
{
    public sealed record SetSettingValue(string SettingsScope, string Name, string Value);
}