using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Settings
{
    [Record(Features.Deconstruct | Features.Builder | Features.ToString | Features.Withers)]
    public sealed partial class SetSettingValue
    {
        public SetSettingValue(string settingsScope, string name, string value)
        {
            SettingsScope = settingsScope;
            Name = name;
            Value = value;
        }

        public string SettingsScope { get; }

        public string Name { get; }

        public string Value { get; }
    }
}