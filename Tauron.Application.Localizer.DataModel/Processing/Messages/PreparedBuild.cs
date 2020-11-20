
namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record PreparedBuild(BuildInfo BuildInfo, Project TargetProject, ProjectFile ProjectFile, string Operation, string TargetPath);
}