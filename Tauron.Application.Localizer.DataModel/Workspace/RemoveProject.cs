namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class RemoveProject
    {
        public Project Project { get; }

        public RemoveProject(Project project) => Project = project;
    }
}