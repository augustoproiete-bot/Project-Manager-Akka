using System;

namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class SaveRequest : EventArgs
    {
        public ProjectFile ProjectFile { get; }

        public SaveRequest(ProjectFile workspace) => ProjectFile = workspace;
    }
}