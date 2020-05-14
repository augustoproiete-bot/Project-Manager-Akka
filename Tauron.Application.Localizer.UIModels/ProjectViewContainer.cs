using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectViewContainer
    {
        [UsedImplicitly]
        public IViewModel Model { get; }

        public Project Project { get; }

        public ProjectViewContainer(IViewModel model, Project project)
        {
            Model = model;
            Project = project;
        }
    }
}