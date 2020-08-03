using System.Collections.Generic;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface INewEntryDialog : IBaseDialog<NewEntryDialogResult?, IEnumerable<NewEntryInfoBase>>
    {
        
    }
}