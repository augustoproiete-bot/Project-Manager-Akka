using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Workspace;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class InitProjectViewModel
    {
        public Project Project { get; }

        public InitProjectViewModel(Project project) => Project = project;
    }
}