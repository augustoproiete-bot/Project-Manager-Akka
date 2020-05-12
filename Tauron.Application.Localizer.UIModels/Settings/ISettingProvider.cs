using System.Collections.Immutable;

namespace Tauron.Application.Localizer.UIModels.Settings
{
    public interface ISettingProvider
    {
        ImmutableDictionary<string, string> Load();

        void Save(ImmutableDictionary<string, string> data);
    }
}