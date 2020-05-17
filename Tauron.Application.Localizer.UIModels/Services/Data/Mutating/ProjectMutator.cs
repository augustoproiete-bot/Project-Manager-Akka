using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.UIModels.Services.Data.Mutating.Changes;
using Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine;

namespace Tauron.Application.Localizer.UIModels.Services.Data.Mutating
{
    [PublicAPI]
    public sealed class ProjectMutator
    {
        private readonly MutatingEngine<MutatingContext> _engine;
        private readonly ProjectFileWorkspace _workspace;

        public IEnumerable<Project> Projects => _workspace.ProjectFile.Projects;

        public ProjectMutator(MutatingEngine<MutatingContext> engine, ProjectFileWorkspace workspace)
        {
            _engine = engine;
            _workspace = workspace;

            NewProject = engine.EventSource(mc => new AddProject(mc.GetChange<NewProjectChange>().Project), context => context.Change is NewProjectChange);
            RemovedProject = engine.EventSource(mc => new RemoveProject(mc.GetChange<RemoveProjectChange>().Project), context => context.Change is RemoveProjectChange);
        }

        public IEventSource<AddProject> NewProject { get; }

        public IEventSource<RemoveProject> RemovedProject { get; }

        public void AddProject(string name)
        {
            _engine.Mutate(nameof(AddProject), 
                context =>
                {
                    var project = new Project(ImmutableList<LocEntry>.Empty, name);
                    var newFile = context.File.AddProject(project);
                    return context.Update(new NewProjectChange(project), newFile);
                });
        }

        public void RemoveProject(string name)
        {
            _engine.Mutate(nameof(RemovedProject),
                context =>
                {
                    var project = context.File.Projects.First(p => p.ProjectName == name);
                    var newFile = context.File.RemoveProject(project);
                    return context.Update(new RemoveProjectChange(project), newFile);
                });
        }
    }
}