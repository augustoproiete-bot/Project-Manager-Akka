using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class RemoveProjectChange : MutatingChange
    {
        public RemoveProjectChange(Project project)
        {
            Project = project;
        }

        public Project Project { get; }
    }
}