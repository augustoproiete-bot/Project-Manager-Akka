namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class RemoveProjectChange : MutatingChange
    {
        public Project Project { get; }

        public RemoveProjectChange(Project project) => Project = project;
    }
}