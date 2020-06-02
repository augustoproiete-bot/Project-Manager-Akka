namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class RemoveImport
    {
        public RemoveImport(string targetProject, string remove)
        {
            TargetProject = targetProject;
            ToRemove = remove;
        }

        public string TargetProject { get; }

        public string ToRemove { get; }
    }
}