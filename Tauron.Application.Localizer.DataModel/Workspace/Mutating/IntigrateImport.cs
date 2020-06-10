namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    public sealed class IntigrateImport
    {
        public bool IsIntigrated { get; }

        public IntigrateImport(bool isIntigrated) => IsIntigrated = isIntigrated;
    }
}