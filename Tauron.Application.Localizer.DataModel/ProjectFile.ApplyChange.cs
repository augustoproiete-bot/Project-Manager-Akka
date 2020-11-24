using Functional.Maybe;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating.Changes;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed partial record ProjectFile : ICanApplyChange<ProjectFile>
    {
        public ProjectFile Apply(MutatingChange apply)
        {
            return apply switch
            {
                IntigrateImportChange intigrateImport => this with { BuildInfo = BuildInfo with{IntigrateProjects = intigrateImport.Switch} },
                ProjectPathChange projectPathChange => this with
                                                           {
                                                           BuildInfo = BuildInfo with
                                                                           {
                                                                           ProjectPaths = BuildInfo.ProjectPaths.SetItem(projectPathChange.TargetProject, 
                                                                                                                         projectPathChange.TargetPath)
                                                                           }
                                                           },
                RemoveEntryChange removeEntryChange => ReplaceEntry(May(removeEntryChange.Entry), Maybe<LocEntry>.Nothing).OrElse(this),
                EntryChange entryChange => ReplaceEntry(May(entryChange.OldEntry), May(entryChange.Entry)).OrElse(this),
                NewEntryChange newEntryChange => ReplaceEntry(Maybe<LocEntry>.Nothing, May(newEntryChange.newEntry)).OrElse(this),
                GlobalLanguageChange globalLanguageChange => this with{GlobalLanguages = GlobalLanguages.Add(globalLanguageChange.Language)},
                NewProjectChange newProjectChange => this with{Projects = Projects.Add(newProjectChange.Project)},
                _ => this
            };
        }
    }
}