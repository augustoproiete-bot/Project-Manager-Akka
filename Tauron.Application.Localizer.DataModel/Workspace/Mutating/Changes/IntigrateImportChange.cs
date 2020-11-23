using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record IntigrateImportChange(bool Switch) : MutatingChange
    {
        public IntigrateImport ToEvent() => new(Switch);
    }
}