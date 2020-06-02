using System;

namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class SaveRequest : EventArgs
    {
        public SaveRequest(ProjectFile workspace)
        {
            ProjectFile = workspace;
        }

        public ProjectFile ProjectFile { get; }
    }
}