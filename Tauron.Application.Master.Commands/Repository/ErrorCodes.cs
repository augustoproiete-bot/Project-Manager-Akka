namespace Tauron.Application.Master.Commands.Repository
{
    public sealed class ErrorCodes
    {
        public const string DuplicateRepository = nameof(DuplicateRepository);

        public const string GithubNoRepoFound = nameof(GithubNoRepoFound);

        public const string InvalidRepoName = nameof(InvalidRepoName);

        public const string DatabaseNoRepoFound = nameof(DatabaseNoRepoFound);
    }
}