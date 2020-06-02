namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class RemoveProject
    {
        public RemoveProject(Project project)
        {
            Project = project;
        }

        public Project Project { get; }
    }
}