using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record ProjectPathChange(string TargetPath, string TargetProject) : MutatingChange
    {
        public ProjectPathChanged ToEventData()
            => new(TargetPath, TargetProject);
    }
}