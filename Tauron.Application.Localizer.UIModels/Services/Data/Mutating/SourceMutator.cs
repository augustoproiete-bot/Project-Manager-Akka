using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.UIModels.Services.Data.Mutating.Changes;
using Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine;

namespace Tauron.Application.Localizer.UIModels.Services.Data.Mutating
{
    [PublicAPI]
    public sealed class SourceMutator
    {
        private readonly MutatingEngine<MutatingContext> _engine;
        private readonly ProjectFileWorkspace _workspace;

        public SourceMutator(MutatingEngine<MutatingContext> engine, ProjectFileWorkspace workspace)
        {
            _engine = engine;
            _workspace = workspace;
            SaveRequest = engine.EventSource(mc => new SaveRequest(_workspace.ProjectFile), context => !(context.Change is ResetChange));
            ProjectReset = engine.EventSource(mc => new ProjectRest(_workspace.ProjectFile), context => context.Change is ResetChange);
        }

        public IEventSource<SaveRequest> SaveRequest { get; }

        public IEventSource<ProjectRest> ProjectReset { get; }

        public void Reset(ProjectFile file)
            => _engine.Mutate(nameof(Reset), context => context.Update(new ResetChange(), file));

        public void UpdateSource(string file)
            => _engine.Mutate(nameof(UpdateSource), context => context.Update(new SourceChange(), context.File.WithSource(file)));
    }
}