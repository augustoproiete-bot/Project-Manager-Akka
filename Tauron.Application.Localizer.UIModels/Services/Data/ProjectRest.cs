using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data
{
    public sealed class ProjectRest
    {
        public ProjectFile ProjectFile { get; }

        public ProjectRest(ProjectFile workspace) => ProjectFile = workspace;
    }
}