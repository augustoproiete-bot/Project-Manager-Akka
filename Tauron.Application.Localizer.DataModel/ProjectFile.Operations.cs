using System.Linq;
using Akka.Actor;
using Functional.Maybe;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed partial record ProjectFile
    {
        public static Maybe<ProjectFile> NewProjectFile(Maybe<IActorContext> factory, Maybe<string> source, Maybe<string> actorName) 
            => from fac in factory
               select FromSource(source, fac.GetOrAdd<ProjectFileOperator>(actorName));

        public Maybe<ProjectFile> AddLanguage(Maybe<Project> mayProject, Maybe<ActiveLanguage> mayLanguage)
            => from project in mayProject
               from lang in mayLanguage
               let newProj = project with{ ActiveLanguages = project.ActiveLanguages.Add(lang) }
               select this with{ Projects = Projects.Replace(project, newProj)};

        public Maybe<ProjectFile> AddProject(Maybe<Project> project)
            => from proj in project
               select this with {Projects = Projects.Add(proj)};

        public Maybe<ProjectFile> RemoveProject(Maybe<Project> project)
            => from proj in project
               select this with{Projects = Projects.Remove(proj)};

        public Maybe<ProjectFile> AddImport(Maybe<Project> project, Maybe<string> toAdd)
            => from proj in project
               from import in toAdd
               let newProj = proj with{Imports = proj.Imports.Add(import)}
               select this with{Projects = Projects.Replace(proj, newProj)};

        public Maybe<ProjectFile> ReplaceEntry(Maybe<LocEntry> mayOldEntry, Maybe<LocEntry> mayNewEntry)
        {
            var validate = mayOldEntry.Or(mayNewEntry);

            var projectName = validate.OrElseDefault()?.Project;
            if (string.IsNullOrWhiteSpace(projectName)) return May(this);
            if (string.IsNullOrWhiteSpace(validate.OrElseDefault()?.Key)) return May(this);

            var mayOld = MayNotNull(Projects.Find(p => p.ProjectName == projectName));

            var modify = Maybe<ProjectFile>.Nothing;

            if (mayOldEntry.IsNothing() && mayNewEntry.IsSomething())
            {
                modify = from old in mayOld
                         from newEntry in mayNewEntry
                         select this with{Projects = Projects.Replace(old, old with{Entries = old.Entries.Add(newEntry)})};
            }
            else if (mayOldEntry.IsSomething() && mayNewEntry.IsNothing())
            {
                modify = from old in mayOld
                         from entry in mayOldEntry
                         select this with{Projects = Projects.Replace(old, old with{Entries = old.Entries.Remove(entry)})};
            }
            else if (mayOldEntry.IsSomething() && mayNewEntry.IsSomething())
            {
                modify = from old in mayOld
                         from oldEntry in mayOldEntry
                         from newEntry in mayNewEntry
                         select this with{Projects = Projects.Replace(old, old with{Entries = old.Entries.Replace(oldEntry, newEntry)})};
            }

            return Either(modify, May(this));
        }
        
        public Maybe<string> FindProjectPath(Maybe<Project> project)
            => from proj in project
               select MayNotEmpty(BuildInfo.ProjectPaths.FirstOrDefault(p => p.Key == proj.ProjectName).Value);
    }
}