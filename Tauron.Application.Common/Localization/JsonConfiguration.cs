using JetBrains.Annotations;

namespace Tauron.Localization
{
    public enum JsonFileNameMode
    {
        Name,
        TwoLetterIsoLanguageName,
        ThreeLetterIsoLanguageName,
        ThreeLetterWindowsLanguageName,
        DisplayName,
        EnglishName
    }

    [PublicAPI]
    public sealed record JsonConfiguration(string RootDic = "lang", JsonFileNameMode NameMode = JsonFileNameMode.Name, string Fallback = "en");
}