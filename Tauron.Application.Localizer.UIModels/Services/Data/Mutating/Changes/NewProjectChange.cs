using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data.Mutating.Changes
{
    public sealed class NewProjectChange : MutatingChange
    {
        public Project Project { get; }

        public NewProjectChange(Project project) => Project = project;
    }
}