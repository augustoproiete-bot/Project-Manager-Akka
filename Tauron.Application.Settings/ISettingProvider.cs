using System.Collections.Immutable;
using Functional.Maybe;

namespace Tauron.Application.Settings
{
    public interface ISettingProvider
    {
        Maybe<ImmutableDictionary<string, string>> Load();

        Maybe<ImmutableDictionary<string, string>> Save(ImmutableDictionary<string, string> data);
    }
}