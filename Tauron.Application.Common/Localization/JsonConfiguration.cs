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
    public sealed class JsonConfiguration
    {
        public string RootDic { get; set; } = "lang";

        public JsonFileNameMode NameMode { get; set; } = JsonFileNameMode.Name;

        public string Fallback { get; set; } = "en";
    }
}