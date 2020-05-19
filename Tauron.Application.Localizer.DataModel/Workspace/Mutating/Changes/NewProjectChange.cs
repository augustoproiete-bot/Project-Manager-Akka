namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class NewProjectChange : MutatingChange
    {
        public Project Project { get; }

        public NewProjectChange(Project project) => Project = project;
    }
}