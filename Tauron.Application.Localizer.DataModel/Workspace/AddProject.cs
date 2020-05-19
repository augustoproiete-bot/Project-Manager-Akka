namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class AddProject
    {
        public Project Project { get; }

        public AddProject(Project project) => Project = project;
    }
}