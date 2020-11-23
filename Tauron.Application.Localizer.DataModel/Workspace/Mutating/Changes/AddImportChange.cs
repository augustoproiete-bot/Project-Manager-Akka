using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public record AddImportChange(string Import, string ProjectName) : MutatingChange
    {
        public AddImport ToEventData() 
            => new(ProjectName, Import);
    }
}