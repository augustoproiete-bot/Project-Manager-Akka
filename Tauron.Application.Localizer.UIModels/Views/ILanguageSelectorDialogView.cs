using System;
using System.Globalization;
using MahApps.Metro.Controls.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface ILanguageSelectorDialogView
    {
        BaseMetroDialog Dialog { get; }

        void Init(Action<CultureInfo?> selector, Predicate<CultureInfo> filter);
    }
}