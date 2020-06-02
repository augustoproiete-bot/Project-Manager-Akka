using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Messages
{
    public sealed class SupplyNewProjectFile
    {
        public SupplyNewProjectFile(ProjectFile file)
        {
            File = file;
        }

        public ProjectFile File { get; }
    }
}