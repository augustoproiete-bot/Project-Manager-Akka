using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class IntigrateImportChange : MutatingChange
    {
        private readonly bool _switch;

        public IntigrateImportChange(bool @switch) => _switch = @switch;

        public IntigrateImport ToEvent() => new IntigrateImport(_switch);
    }
}