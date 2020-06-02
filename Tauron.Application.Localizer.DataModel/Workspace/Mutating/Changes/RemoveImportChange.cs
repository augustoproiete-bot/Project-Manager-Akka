using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class RemoveImportChange : MutatingChange
    {
        public RemoveImportChange(string targetProject, string remove)
        {
            TargetProject = targetProject;
            ToRemove = remove;
        }

        private string TargetProject { get; }

        private string ToRemove { get; }

        public RemoveImport ToData()
        {
            return new RemoveImport(TargetProject, ToRemove);
        }
    }
}