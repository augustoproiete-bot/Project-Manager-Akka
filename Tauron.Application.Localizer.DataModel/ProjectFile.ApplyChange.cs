using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating.Changes;

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
                _ => this
            };
        }
    }
}