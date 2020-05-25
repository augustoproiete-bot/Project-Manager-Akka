using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Localizer.DataModel.Workspace
{
    [PublicAPI]
    public sealed class ProjectFileWorkspace : Workspace<ProjectFileWorkspace, ProjectFile>
    {
        private ProjectFile _projectFile;

        public ProjectFile ProjectFile => _projectFile;

        public SourceMutator Source { get; }

        public ProjectMutator Projects { get; }

        public ProjectFileWorkspace(IActorRefFactory factory)
            : base(factory)
        {
            _projectFile = new ProjectFile();

            Projects = new ProjectMutator(Engine, this);
            Source = new SourceMutator(Engine, this);

            Analyzer.RegisterRule(new SourceRule());
        }


        public Project Get(string name) => ProjectFile.Projects.Find(p => p.ProjectName == name);
        protected override MutatingContext<ProjectFile> GetDataInternal() => new MutatingContext<ProjectFile>(null, _projectFile);

        protected override void SetDataInternal(MutatingContext<ProjectFile> data) => Interlocked.Exchange(ref _projectFile, data.Data);
    }
}