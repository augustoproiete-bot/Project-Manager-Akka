using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing;
using Tauron.Application.Localizer.DataModel.Workspace.Analyzing.Rules;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating;
using Tauron.Application.Localizer.DataModel.Workspace.MutatingEngine;

namespace Tauron.Application.Localizer.DataModel.Workspace
{
    [PublicAPI]
    public sealed class ProjectFileWorkspace : IDataSource<MutatingContext>
    {
        public static readonly ProjectFileWorkspace Dummy = new ProjectFileWorkspace();

        private ProjectFile _projectFile;
        private MutatingEngine<MutatingContext> _mutatingEngine;

        public ProjectFile ProjectFile => _projectFile;

        public Analyzer Analyzer { get; }

        public SourceMutator Source { get; }

        public ProjectMutator Projects { get; }

        private ProjectFileWorkspace()
        {
            Analyzer = new Analyzer();

            _mutatingEngine = new MutatingEngine<MutatingContext>(this);

            Projects = new ProjectMutator(_mutatingEngine, this);
            Source = new SourceMutator(_mutatingEngine, this);

            _projectFile = new ProjectFile();
        }

        public ProjectFileWorkspace(IUntypedActorContext factory)
        {
            _projectFile = new ProjectFile();
            Analyzer = new Analyzer(this, factory);
            _mutatingEngine = new MutatingEngine<MutatingContext>(factory, this);

            Projects = new ProjectMutator(_mutatingEngine, this);
            Source = new SourceMutator(_mutatingEngine, this);

            Analyzer.RegisterRule(new SourceRule());
        }

        MutatingContext IDataSource<MutatingContext>.GetData() => new MutatingContext(null, ProjectFile);

        void IDataSource<MutatingContext>.SetData(MutatingContext data) => Interlocked.Exchange(ref _projectFile, data.File);

        public Project Get(string name) => ProjectFile.Projects.Find(p => p.ProjectName == name);
    }
}