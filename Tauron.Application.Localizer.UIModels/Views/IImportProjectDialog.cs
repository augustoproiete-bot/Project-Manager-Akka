using System.Collections.Generic;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface IImportProjectDialog : IBaseDialog<ImportProjectDialogResult?, IEnumerable<string>>
    {
    }
}