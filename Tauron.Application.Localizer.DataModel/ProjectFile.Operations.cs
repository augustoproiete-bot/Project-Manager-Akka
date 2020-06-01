using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed partial class ProjectFile
    {
        public static ProjectFile NewProjectFile(IActorContext factory, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);
            return FromSource(source, actor);
        }

        public ProjectFile AddLanguage(Project project, ActiveLanguage language)
        {
            var temp = project.WithActiveLanguages(project.ActiveLanguages.Add(language));
            return WithProjects(Projects.Replace(project, temp));
        }

        public ProjectFile AddProject(Project project)
            => WithProjects(Projects.Add(project));

        public ProjectFile RemoveProject(Project project)
            => WithProjects(Projects.Remove(project));

        public ProjectFile AddImport(Project project, string toAdd)
            => WithProjects(Projects.Replace(project, project.WithImports(project.Imports.Add(toAdd))));

        public ProjectFile ReplaceEntry(LocEntry? oldEntry, LocEntry? newEntry)
        {
            var projectName = oldEntry?.Project ?? newEntry?.Project;
            if (string.IsNullOrWhiteSpace(projectName)) return this;

            var entryName = oldEntry?.Key ?? newEntry?.Key;
            if (string.IsNullOrWhiteSpace(entryName)) return this;

            var old = Projects.Find(p => p.ProjectName == projectName);

            if (oldEntry == null && newEntry != null)
                return WithProjects(Projects.Replace(old, old.WithEntries(old.Entries.Add(newEntry))));
            if (oldEntry != null && newEntry == null)
                return WithProjects(Projects.Replace(old, old.WithEntries(old.Entries.Remove(oldEntry))));
            if (oldEntry != null && newEntry != null)
                return WithProjects(Projects.Replace(old, old.WithEntries(old.Entries.Replace(oldEntry, newEntry))));

            return this;
        }
    }
}