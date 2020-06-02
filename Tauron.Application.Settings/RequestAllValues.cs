using Amadevus.RecordGenerator;

namespace Tauron.Application.Settings
{
    [Record(Features.Deconstruct | Features.Builder | Features.ToString | Features.Withers)]
    public sealed partial class RequestAllValues
    {
        public RequestAllValues(string settingScope)
        {
            SettingScope = settingScope;
        }

        public string SettingScope { get; }
    }
}