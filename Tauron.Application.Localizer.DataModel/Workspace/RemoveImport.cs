namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class RemoveImport
    {
        public string TargetProject { get; }

        public string ToRemove { get; }

        public RemoveImport(string targetProject, string remove)
        {
            TargetProject = targetProject;
            ToRemove = remove;
        }
    }
}