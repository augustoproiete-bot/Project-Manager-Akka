using System.Globalization;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class AddLanguageDialogResult
    {
        public AddLanguageDialogResult(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
        }

        public CultureInfo CultureInfo { get; }
    }
}