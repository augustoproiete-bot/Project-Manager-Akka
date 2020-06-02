namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class ProjectRest
    {
        public ProjectRest(ProjectFile workspace)
        {
            ProjectFile = workspace;
        }

        public ProjectFile ProjectFile { get; }
    }
}