using System;
using System.Globalization;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface ILanguageSelectorDialog : IBaseDialog<AddLanguageDialogResult?, CultureInfo>
    {
    }
}