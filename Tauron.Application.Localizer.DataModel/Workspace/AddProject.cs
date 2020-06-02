namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class AddProject
    {
        public AddProject(Project project)
        {
            Project = project;
        }

        public Project Project { get; }
    }
}