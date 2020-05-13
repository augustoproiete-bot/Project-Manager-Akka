using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Messages
{
    public sealed class SupplyNewProjectFile
    {
        public ProjectFile File { get; }

        public SupplyNewProjectFile(ProjectFile file) => File = file;
    }
}