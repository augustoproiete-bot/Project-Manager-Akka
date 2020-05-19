namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class ProjectRest
    {
        public ProjectFile ProjectFile { get; }

        public ProjectRest(ProjectFile workspace) => ProjectFile = workspace;
    }
}