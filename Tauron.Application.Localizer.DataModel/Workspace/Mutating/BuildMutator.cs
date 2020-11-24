using Functional.Maybe;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    public sealed class BuildMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;

        public BuildMutator(MutatingEngine<MutatingContext<ProjectFile>> engine)
        {
            _engine = engine;

            Intigrate = engine.EventSource(mc =>
                                               from context in mc
                                               select context.GetChange<IntigrateImportChange>().ToEvent(),
                                           mc =>
                                               from context in mc
                                               from change in context.Change
                                               select change is IntigrateImportChange);

            ProjectPath = engine.EventSource(mc =>
                                                 from context in mc
                                                 select context.GetChange<ProjectPathChange>().ToEventData(),
                                             mc =>
                                                 from context in mc
                                                 from change in context.Change
                                                 select change is ProjectPathChange);
        }

        public IEventSource<IntigrateImport> Intigrate { get; }

        public IEventSource<ProjectPathChanged> ProjectPath { get; }

        public Maybe<Unit> SetIntigrate(bool intigrate)
            => _engine.Mutate(nameof(SetIntigrate),
                              mc =>
                                  from context in mc
                                  where context.Data.BuildInfo.IntigrateProjects != intigrate
                                  select context.WithChange(May(new IntigrateImportChange(intigrate))));


        public Maybe<Unit> SetProjectPath(string project, string path)
            => _engine.Mutate(nameof(SetProjectPath),
                              mc =>
                                  from context in mc
                                  from currentPath in context.Data.BuildInfo.ProjectPaths.Lookup(project)
                                  where currentPath != path
                                  select context.WithChange(new ProjectPathChange(path, project)));

    }
}