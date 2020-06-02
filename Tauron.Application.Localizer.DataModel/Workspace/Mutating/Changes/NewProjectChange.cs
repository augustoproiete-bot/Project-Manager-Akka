using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class NewProjectChange : MutatingChange
    {
        public NewProjectChange(Project project)
        {
            Project = project;
        }

        public Project Project { get; }
    }
}