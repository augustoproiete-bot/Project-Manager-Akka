using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface IBaseDialog<TData, in TViewData>
    {
        BaseMetroDialog Dialog { get; }

        Task<TData> Init(IEnumerable<TViewData> initalData);
    }
}