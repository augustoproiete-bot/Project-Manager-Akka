using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data.Mutating.Changes
{
    public sealed class RemoveProjectChange : MutatingChange
    {
        public Project Project { get; }

        public RemoveProjectChange(Project project) => Project = project;
    }
}