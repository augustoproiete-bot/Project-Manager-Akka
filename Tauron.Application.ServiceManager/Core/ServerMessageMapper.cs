using Tauron.Application.Localizer.Generated;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace Tauron.Application.ServiceManager.Core
{
    public static class ServerMessageMapper
    {
        public static string GetMessageString(string? msg)
        {
            var loc = LocLocalizer.Inst;

            return msg switch
            {
                RepositoryMessages.CompressRepository => loc.RepositoryMessages.CompressRepository,
                RepositoryMessages.DownloadRepository => loc.RepositoryMessages.DownloadRepository,
                RepositoryMessages.ExtractRepository => loc.RepositoryMessages.ExtractRepository,
                RepositoryMessages.GetRepo => loc.RepositoryMessages.GetRepo,
                RepositoryMessages.GetRepositoryFromDatabase => loc.RepositoryMessages.GetRepositoryFromDatabase,
                RepositoryMessages.UpdateRepository => loc.RepositoryMessages.UpdateRepository,
                RepositoryMessages.UploadRepositoryToDatabase => loc.RepositoryMessages.UploadRepositoryToDatabase,

                RepoErrorCodes.DatabaseNoRepoFound => loc.RepoErrorCodes.DatabaseNoRepoFound,
                RepoErrorCodes.DuplicateRepository => loc.RepoErrorCodes.DuplicateRepository,
                RepoErrorCodes.GithubNoRepoFound => loc.RepoErrorCodes.GithubNoRepoFound,
                RepoErrorCodes.InvalidRepoName => loc.RepoErrorCodes.InvalidRepoName,

                DeploymentMessages.BuildCompled => loc.DeploymentMessages.BuildCompled,
                DeploymentMessages.BuildExtractingRepository => loc.DeploymentMessages.BuildExtractingRepository,
                DeploymentMessages.BuildKilling => loc.DeploymentMessages.BuildKilling,
                DeploymentMessages.BuildRunBuilding => loc.DeploymentMessages.BuildRunBuilding,
                DeploymentMessages.BuildStart => loc.DeploymentMessages.BuildStart,
                DeploymentMessages.BuildTryFindProject => loc.DeploymentMessages.BuildTryFindProject,
                DeploymentMessages.RegisterRepository => loc.DeploymentMessages.RegisterRepository,

                BuildErrorCodes.BuildDotNetFailed => loc.BuildErrorCodes.BuildDotNetFailed,
                BuildErrorCodes.BuildDotnetNotFound => loc.BuildErrorCodes.BuildDotnetNotFound,
                BuildErrorCodes.BuildProjectNotFound => loc.BuildErrorCodes.BuildProjectNotFound,
                BuildErrorCodes.CommandAppNotFound => loc.BuildErrorCodes.CommandAppNotFound,
                BuildErrorCodes.CommandDuplicateApp => loc.BuildErrorCodes.CommandDuplicateApp,
                BuildErrorCodes.CommandErrorRegisterRepository => loc.BuildErrorCodes.CommandErrorRegisterRepository,
                BuildErrorCodes.DatabaseError => loc.BuildErrorCodes.DatabaseError,
                BuildErrorCodes.GeneralQueryFailed => loc.BuildErrorCodes.GeneralQueryFailed,
                BuildErrorCodes.GernalBuildError => loc.BuildErrorCodes.GernalBuildError,
                BuildErrorCodes.QueryAppNotFound => loc.BuildErrorCodes.QueryAppNotFound,
                BuildErrorCodes.QueryFileNotFound => loc.BuildErrorCodes.QueryFileNotFound,
                BuildErrorCodes.GerneralCommandError => loc.BuildErrorCodes.GerneralCommandError,

                null => loc.Common.Unkowen,
                "" => loc.Common.Unkowen,
                _ => msg
            };
        }
    }
}