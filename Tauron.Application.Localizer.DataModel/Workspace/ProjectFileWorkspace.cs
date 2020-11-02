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

        public ProjectFileWorkspace(IActorRefFactory factory)
            : base(new WorkspaceSuperviser(factory, "Project_File_Workspace"))
        {
            _projectFile = new ProjectFile();

            Projects = new ProjectMutator(Engine, this);
            Source = new SourceMutator(Engine, this);
            Entrys = new EntryMutator(Engine);
            Build = new BuildMutator(Engine);

            Analyzer.RegisterRule(new SourceRule());
        }

        public ProjectFile ProjectFile => _projectFile;

        public SourceMutator Source { get; }

        public ProjectMutator Projects { get; }

        public EntryMutator Entrys { get; }

        public BuildMutator Build { get; }

        public Project Get(string name) 
            => ProjectFile.Projects.Find(p => p.ProjectName == name) ?? new Project();

        protected override MutatingContext<ProjectFile> GetDataInternal() 
            => MutatingContext<ProjectFile>.New(_projectFile);

        protected override void SetDataInternal(MutatingContext<ProjectFile> data) 
            => Interlocked.Exchange(ref _projectFile, data.Data);
    }
}