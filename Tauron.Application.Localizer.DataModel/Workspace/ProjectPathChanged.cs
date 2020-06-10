namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class ProjectPathChanged
    {
        public string TargetPath { get; }

        public string TargetProject { get; }

        public ProjectPathChanged(string targetPath, string targetProject)
        {
            TargetPath = targetPath;
            TargetProject = targetProject;
        }
    }
}