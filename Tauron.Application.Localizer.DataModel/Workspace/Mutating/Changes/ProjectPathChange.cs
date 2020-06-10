using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes
{
    public sealed class ProjectPathChange : MutatingChange
    {
        private readonly string _targetPath;

        private readonly string _targetProject;

        public ProjectPathChange(string targetPath, string targetProject)
        {
            _targetPath = targetPath;
            _targetProject = targetProject;
        }

        public ProjectPathChanged ToEventData()
            => new ProjectPathChanged(_targetPath, _targetProject);
    }
}