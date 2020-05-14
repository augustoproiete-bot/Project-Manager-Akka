using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.UIModels.Services.Data.Mutating;
using Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine;

namespace Tauron.Application.Localizer.UIModels.Services.Data
{
    [PublicAPI]
    public sealed class ProjectFileWorkspace : IDataSource<MutatingContext>
    {
        private ProjectFile _projectFile;
        private MutatingEngine<MutatingContext> _mutatingEngine;

        public ProjectFile ProjectFile => _projectFile;

        public SourceMutator Source { get; }

        public ProjectFileWorkspace(IActorRefFactory factory)
        {
            _projectFile = new ProjectFile();
            _mutatingEngine = new MutatingEngine<MutatingContext>(factory, this);

            Source = new SourceMutator(_mutatingEngine, this);
        }

        MutatingContext IDataSource<MutatingContext>.GetData() => new MutatingContext(null, ProjectFile);

        void IDataSource<MutatingContext>.SetData(MutatingContext data) => Interlocked.Exchange(ref _projectFile, data.File);
    }
}