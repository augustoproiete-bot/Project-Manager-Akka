using System;
using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data
{
    public sealed class SaveRequest : EventArgs
    {
        public ProjectFile ProjectFile { get; }

        public SaveRequest(ProjectFile workspace) => ProjectFile = workspace;
    }
}