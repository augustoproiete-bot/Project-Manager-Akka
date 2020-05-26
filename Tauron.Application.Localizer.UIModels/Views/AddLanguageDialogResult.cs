using System.Globalization;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class AddLanguageDialogResult
    {
        public CultureInfo CultureInfo { get; }

        public AddLanguageDialogResult(CultureInfo cultureInfo) => CultureInfo = cultureInfo;
    }
}