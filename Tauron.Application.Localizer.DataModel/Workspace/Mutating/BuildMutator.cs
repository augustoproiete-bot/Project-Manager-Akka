using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    public sealed class BuildMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;

        public BuildMutator(MutatingEngine<MutatingContext<ProjectFile>> engine)
        {
            _engine = engine;
            Intigrate = engine.EventSource(mc => mc.GetChange<IntigrateImportChange>().ToEvent(), mc => mc.Change is IntigrateImportChange);
        }

        public IEventSource<IntigrateImport> Intigrate { get; }

        public void SetIntigrate(bool intigrate)
        {
            _engine.Mutate(nameof(SetIntigrate),
                mc => mc.Data.BuildInfo.IntigrateProjects == intigrate 
                    ? mc 
                    : mc.Update(new IntigrateImportChange(intigrate), mc.Data.WithBuildInfo(mc.Data.BuildInfo.WithIntigrateProjects(intigrate))));
        }
    }
}