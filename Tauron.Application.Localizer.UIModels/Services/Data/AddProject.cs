using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data
{
    public sealed class AddProject
    {
        public Project Project { get; }

        public AddProject(Project project) => Project = project;
    }
}