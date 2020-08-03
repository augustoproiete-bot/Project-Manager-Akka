using System.Collections.Generic;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.ServiceManager.ViewModels.Dialogs
{

    public interface IAddSeedUrlDialog : IBaseDialog<DialogSeedEntry, IEnumerable<DialogSeedEntry>>
    {
    }
}