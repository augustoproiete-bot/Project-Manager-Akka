using System;
using System.Collections.Generic;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface IImportProjectDialog
    {
        void Init(Action<string?> selector, IEnumerable<string> projects);
    }
}