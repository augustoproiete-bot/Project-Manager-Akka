using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectViewContainer
    {
        public ProjectViewContainer(IViewModel model, Project project)
        {
            Model = model;
            Project = project;
        }

        [UsedImplicitly] public IViewModel Model { get; }

        public Project Project { get; }
    }
}