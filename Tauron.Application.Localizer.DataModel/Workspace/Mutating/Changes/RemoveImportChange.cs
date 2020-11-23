using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed record RemoveImportChange(string TargetProject, string ToRemove) : MutatingChange
    {
        public RemoveImport ToData() => new(TargetProject, ToRemove);
    }
}