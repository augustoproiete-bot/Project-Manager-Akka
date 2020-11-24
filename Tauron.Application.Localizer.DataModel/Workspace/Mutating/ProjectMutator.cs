using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [PublicAPI]
    public sealed class ProjectMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;
        private readonly ProjectFileWorkspace _workspace;

        public ProjectMutator(MutatingEngine<MutatingContext<ProjectFile>> engine, ProjectFileWorkspace workspace)
        {
            _engine = engine;
            _workspace = workspace;

            NewProject = engine.EventSource(
                                            c => c.Select(mc => new AddProject(mc.GetChange<NewProjectChange>().Project)),
                                            c => from context in c
                                                 from change in context.Change 
                                                 select change is NewProjectChange);
            
            RemovedProject = engine.EventSource(c => c.Select(mc =>  new RemoveProject(mc.GetChange<RemoveProjectChange>().Project)),
                                                c => from context in c
                                                     from change in context.Change 
                                                     select change is RemoveProjectChange);
            
            NewLanguage = engine.EventSource(c => c.Select(mc => mc.GetChange<LanguageChange>().ToEventData()),
                                             c => from context in c
                                                  from change in context.Change 
                                                  select change is LanguageChange);
            
            NewImport = engine.EventSource(c => c.Select(mc => mc.GetChange<AddImportChange>().ToEventData()),
                                           c => from context in c
                                                from change in context.Change 
                                                select change is AddImportChange);
            
            RemoveImport = engine.EventSource(c => c.Select(mc => mc.GetChange<RemoveImportChange>().ToData()),
                                              c => from context in c
                                                   from change in context.Change 
                                                   select change is RemoveImportChange);

            NewLanguage.RespondOn(null, mayNewLang =>
                                            Do(from newlang in mayNewLang
                                               where !workspace.ProjectFile.GlobalLanguages.Contains(newlang.ActiveLanguage)
                                               where Projects.All(p => p.ActiveLanguages.Contains(newlang.ActiveLanguage))
                                               select Action(() => _engine.Mutate(nameof(AddLanguage) + "Global-Single",
                                                                  c => from context in c 
                                                                       select context.WithChange(new GlobalLanguageChange(newlang.ActiveLanguage))))));
        }

        public IEnumerable<Project> Projects => _workspace.ProjectFile.Projects;

        public IEventSource<AddProject> NewProject { get; }

        public IEventSource<RemoveProject> RemovedProject { get; }

        public IEventSource<AddActiveLanguage> NewLanguage { get; }

        public IEventSource<AddImport> NewImport { get; }

        public IEventSource<RemoveImport> RemoveImport { get; }

        public Maybe<Unit> AddProject(string mayName)
            => _engine.Mutate(nameof(AddProject),
                              c => from context in c
                                   from name in MayNotEmpty(mayName)
                                   let newProject = new Project(name, ImmutableList.CreateRange(context.Data.GlobalLanguages))
                                   select context.WithChange(new NewProjectChange(newProject)));

        public Maybe<Unit> RemoveProject(string name)
            =>
        // {
        //     _engine.Mutate(nameof(RemovedProject),
        //         context =>
        //         {
        //             var project = context.Data.Projects.First(p => p.ProjectName == name);
        //             var newFile = context.Data.RemoveProject(project);
        //             return context.Update(new RemoveProjectChange(project), newFile);
        //         });
        // }

        public Maybe<Unit> AddLanguage(CultureInfo? info)
        {
            if (info == null) return;

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

        public Maybe<Unit> AddLanguage(string proj, CultureInfo info)
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

        public Maybe<Unit> AddImport(string projectName, string toAdd)
        {
            if (projectName == toAdd)
                return;

            _engine.Mutate(nameof(AddImport), context =>
            {
                var project = context.Data.Projects.First(p => p.ProjectName == projectName);
                if (project.Imports.Contains(toAdd) || context.Data.Projects.All(p => toAdd != p.ProjectName)) return context;

                return context.Update(new AddImportChange(toAdd, projectName), context.Data.AddImport(project, toAdd));
            });
        }

        public Maybe<Unit> TryRemoveImport(string projectName, string toRemove)
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