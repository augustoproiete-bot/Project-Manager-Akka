using System.Collections.Generic;
using System.Globalization;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface ILanguageSelectorDialog : IBaseDialog<AddLanguageDialogResult?, IEnumerable<CultureInfo>>
    {
    }
}