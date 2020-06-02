using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

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
            RemoveImport = engine.EventSource(mc => mc.GetChange<RemoveImportChange>().ToData(), context => context.Change is RemoveImportChange);

            NewLanguage.RespondOn(newLang =>
            {
                if(workspace.ProjectFile.GlobalLanguages.Contains(newLang.ActiveLanguage)) return;

                if(!Projects.All(p => p.ActiveLanguages.Contains(newLang.ActiveLanguage))) return;
                
                _engine.Mutate(nameof(AddLanguage) + "Global-Single", 
                    context => context.Update(
                        new GlobalLanguageChange(newLang.ActiveLanguage), 
                        context.Data.WithGlobalLanguages(context.Data.GlobalLanguages.Add(newLang.ActiveLanguage))));
            });
        }

        public IEventSource<AddProject> NewProject { get; }

        public IEventSource<RemoveProject> RemovedProject { get; }

        public IEventSource<AddActiveLanguage> NewLanguage { get; }

        public IEventSource<AddImport> NewImport { get; }

        public IEventSource<RemoveImport> RemoveImport { get; }

        public void AddProject(string name)
        {
            if(string.IsNullOrWhiteSpace(name)) return;

            _engine.Mutate(nameof(AddProject), 
                context =>
                {
                    var project = new Project(name).WithActiveLanguages(ImmutableList.CreateRange(context.Data.GlobalLanguages));
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

        public void AddLanguage(CultureInfo? info)
        {
            if(info == null) return;

            var newLang = ActiveLanguage.FromCulture(info);

            _engine.Mutate(nameof(AddLanguage) + "Global",
                context =>
                {
                    context = context.Update(
                        new GlobalLanguageChange(newLang),
                        context.Data.WithGlobalLanguages(context.Data.GlobalLanguages.Add(newLang)));

                    foreach (var project in context.Data.Projects.Select(p => p.ProjectName))
                        AddLanguage(project, info);

                    return context;
                });

        }

        public void AddLanguage(string proj, CultureInfo info)
        {
            _engine.Mutate(nameof(AddLanguage), context =>
                                                {
                                                    var project = context.Data.Projects.First(p => p.ProjectName == proj);
                                                    var lang = ActiveLanguage.FromCulture(info);
                                                    return project.ActiveLanguages.Contains(lang) 
                                                        ? context 
                                                        : context.Update(new LanguageChange(lang, proj), context.Data.AddLanguage(project, lang));
                                                });
        }

        public void AddImport(string projectName, string toAdd)
        {
            if(projectName == toAdd)
                return;

            _engine.Mutate(nameof(AddImport), context =>
            {
                var project = context.Data.Projects.First(p => p.ProjectName == projectName);
                if (project.Imports.Contains(toAdd) || context.Data.Projects.All(p => toAdd != p.ProjectName)) return context;

                return context.Update(new AddImportChange(toAdd, projectName), context.Data.AddImport(project, toAdd));
            });
        }

        public void TryRemoveImport(string projectName, string toRemove)
        {
            _engine.Mutate(nameof(RemoveImport), context =>
                                                 {
                                                     var pro = context.Data.Projects.Find(p => p.ProjectName == projectName);
                                                     if (pro == null) return context;
                                                     if (!pro.Imports.Contains(toRemove)) return context;

                                                     var newData = context.Data.WithProjects(context.Data.Projects.Replace(pro, pro.WithImports(pro.Imports.Remove(toRemove))));
                                                     return context.Update(new RemoveImportChange(projectName, toRemove), newData);
                                                 });
        }
    }
}