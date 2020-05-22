using System;
using System.Collections.Generic;
using MahApps.Metro.Controls.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface IImportProjectDialog
    {
        void Init(Action<string?> selector, IEnumerable<string> projects);
        BaseMetroDialog Dialog { get; }
    }
}