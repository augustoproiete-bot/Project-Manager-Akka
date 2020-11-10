namespace Tauron.Application.Master.Commands.Deployment.Build
{
    public static class BuildErrorCodes
    {
        public const string GeneralQueryFailed = nameof(GeneralQueryFailed);

        public const string QueryAppNotFound = nameof(QueryAppNotFound);

        public const string QueryFileNotFound = nameof(QueryFileNotFound);

        public const string GerneralCommandError = nameof(GerneralCommandError);

        public const string CommandErrorRegisterRepository = nameof(CommandErrorRegisterRepository);

        public const string CommandDuplicateApp = nameof(CommandDuplicateApp);

        public const string CommandAppNotFound = nameof(CommandAppNotFound);

        public const string GernalBuildError = nameof(GernalBuildError);

        public const string BuildDotnetNotFound = nameof(BuildDotnetNotFound);

        public const string BuildDotNetFailed = nameof(BuildDotNetFailed);

        public const string BuildProjectNotFound = nameof(BuildProjectNotFound);

        public const string DatabaseError = nameof(DatabaseError);
    }
}