using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class RemoveImportChange : MutatingChange
    {
        private string TargetProject { get; }

        private string ToRemove { get; }

        public RemoveImportChange(string targetProject, string remove)
        {
            TargetProject = targetProject;
            ToRemove = remove;
        }

        public RemoveImport ToData() => new RemoveImport(TargetProject, ToRemove);
    }
}