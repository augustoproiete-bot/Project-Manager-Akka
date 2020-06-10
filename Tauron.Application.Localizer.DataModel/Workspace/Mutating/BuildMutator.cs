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
            ProjectPath = engine.EventSource(mc => mc.GetChange<ProjectPathChange>().ToEventData(), context => context.Change is ProjectPathChange);
        }

        public IEventSource<IntigrateImport> Intigrate { get; }

        public IEventSource<ProjectPathChanged> ProjectPath { get; }

        public void SetIntigrate(bool intigrate)
        {
            _engine.Mutate(nameof(SetIntigrate),
                mc => mc.Data.BuildInfo.IntigrateProjects == intigrate 
                    ? mc 
                    : mc.Update(new IntigrateImportChange(intigrate), mc.Data.WithBuildInfo(mc.Data.BuildInfo.WithIntigrateProjects(intigrate))));
        }

        public void SetProjectPath(string project, string path)
        {
            _engine.Mutate(nameof(SetProjectPath),
                mc =>
                {
                    if (mc.Data.BuildInfo.ProjectPaths.TryGetValue(project, out var settetPath) && settetPath == path)
                        return mc;

                    return mc.Update(new ProjectPathChange(path, project), mc.Data.WithBuildInfo(mc.Data.BuildInfo.WithProjectPaths(mc.Data.BuildInfo.ProjectPaths.SetItem(project, path))));
                });
        }
    }
}