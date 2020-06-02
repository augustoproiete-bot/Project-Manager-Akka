using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public class AddImportChange : MutatingChange
    {
        public AddImportChange(string import, string projectName)
        {
            Import = import;
            ProjectName = projectName;
        }

        private string Import { get; }

        private string ProjectName { get; }

        public AddImport ToEventData()
        {
            return new AddImport(ProjectName, Import);
        }
    }
}