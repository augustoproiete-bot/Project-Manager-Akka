using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data
{
    public sealed class RemoveProject
    {
        public Project Project { get; }

        public RemoveProject(Project project) => Project = project;
    }
}