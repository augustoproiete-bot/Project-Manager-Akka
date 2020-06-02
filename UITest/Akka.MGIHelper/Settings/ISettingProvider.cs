using System.Collections.Immutable;

namespace Akka.MGIHelper.Settings
{
    public interface ISettingProvider
    {
        ImmutableDictionary<string, string> Load();

        void Save(ImmutableDictionary<string, string> data);
    }
}