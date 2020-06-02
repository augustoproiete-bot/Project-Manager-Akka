using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class InitProjectViewModel
    {
        public InitProjectViewModel(Project project)
        {
            Project = project;
        }

        public Project Project { get; }
    }
}