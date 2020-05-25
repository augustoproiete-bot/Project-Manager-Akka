using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public class AddImportChange : MutatingChange
    {
        private string Import { get; }

        private string ProjectName { get; }

        public AddImportChange(string import, string projectName)
        {
            Import = import;
            ProjectName = projectName;
        }

        public AddImport ToEventData() => new AddImport(ProjectName, Import);
    }
}