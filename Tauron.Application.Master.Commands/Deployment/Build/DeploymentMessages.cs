namespace Tauron.Application.Master.Commands.Deployment.Build
{
    public static class DeploymentMessages
    {
        public const string RegisterRepository = "RegisterRepository";

        public const string BuildStart = nameof(BuildStart);

        public const string BuildKilling = nameof(BuildKilling);

        public const string BuildCompled = nameof(BuildCompled);

        //public const string BuildTransferRepoistory = nameof(BuildTransferRepoistory);

        public const string BuildExtractingRepository = nameof(BuildExtractingRepository);

        public const string BuildRunBuilding = nameof(BuildRunBuilding);

        public const string BuildTryFindProject = nameof(BuildTryFindProject);
    }
}