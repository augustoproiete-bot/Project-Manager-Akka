using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.MutatingEngine;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [PublicAPI]
    public sealed class ProjectMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;
        private readonly ProjectFileWorkspace _workspace;

        public IEnumerable<Project> Projects => _workspace.ProjectFile.Projects;

        public ProjectMutator(MutatingEngine<MutatingContext<ProjectFile>> engine, ProjectFileWorkspace workspace)
        {
            _engine = engine;
            _workspace = workspace;

            NewProject = engine.EventSource(mc => new AddProject(mc.GetChange<NewProjectChange>().Project), context => context.Change is NewProjectChange);
            RemovedProject = engine.EventSource(mc => new RemoveProject(mc.GetChange<RemoveProjectChange>().Project), context => context.Change is RemoveProjectChange);
            NewLanguage = engine.EventSource(mc => mc.GetChange<LanguageChange>().ToEventData(), context => context.Change is LanguageChange);
            NewImport = engine.EventSource(mc => mc.GetChange<AddImportChange>().ToEventData(), context => context.Change is AddImportChange);
        }

        public IEventSource<AddProject> NewProject { get; }

        public IEventSource<RemoveProject> RemovedProject { get; }

        public IEventSource<AddActiveLanguage> NewLanguage { get; }

        public IEventSource<AddImport> NewImport { get; }

        public void AddProject(string name)
        {
            _engine.Mutate(nameof(AddProject), 
                context =>
                {
                    var project = new Project(name);
                    var newFile = context.Data.AddProject(project);
                    return context.Update(new NewProjectChange(project), newFile);
                });
        }

        public void RemoveProject(string name)
        {
            _engine.Mutate(nameof(RemovedProject),
                context =>
                {
                    var project = context.Data.Projects.First(p => p.ProjectName == name);
                    var newFile = context.Data.RemoveProject(project);
                    return context.Update(new RemoveProjectChange(project), newFile);
                });
        }

        public void AddLanguage(string proj, CultureInfo info)
        {
            _engine.Mutate(nameof(AddLanguage), context =>
                                                {
                                                    var project = context.Data.Projects.First(p => p.ProjectName == proj);
                                                    var lang = ActiveLanguage.FromCulture(info);
                                                    return context.Update(new LanguageChange(lang, proj), context.Data.AddLanguage(project, lang));
                                                });
        }

        public void AddImport(string projectName, string toAdd)
        {
            if(projectName == toAdd)
                return;

            _engine.Mutate(nameof(AddImport), context =>
            {
                var project = context.Data.Projects.First(p => p.ProjectName == projectName);
                if (project.Imports.Contains(toAdd) || context.Data.Projects.Any(p => toAdd == p.ProjectName)) return context;

                return context.Update(new AddImportChange(toAdd, projectName), context.Data.AddImport(project, toAdd));
            });
        }
    }
}