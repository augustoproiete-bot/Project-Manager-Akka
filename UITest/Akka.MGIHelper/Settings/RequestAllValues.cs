using System.Collections.Immutable;
using Amadevus.RecordGenerator;

namespace Akka.MGIHelper.Settings
{
    [Record(Features.Deconstruct | Features.Builder | Features.ToString | Features.Withers)]
    public sealed partial class RequestAllValues
    {
        public string SettingScope { get; }

        public RequestAllValues(string settingScope) => SettingScope = settingScope;
    }
}