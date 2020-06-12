namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class BuildMessage
    {
        public static class Ids
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            public const string GatherData = nameof(GatherData);

            public const string NoData = nameof(NoData);

            public const string GenerateLangFiles = nameof(GenerateLangFiles);

            public const string GenerateCsFiles = nameof(GenerateCsFiles);

            public const string AgentCompled = nameof(AgentCompled);
            // ReSharper restore MemberHidesStaticFromOuterClass
        }

        public string Message { get; }

        public string OperationId { get; }

        public string Agent { get; }

        private BuildMessage(string message, string operationId, string agent)
        {
            Message = message;
            OperationId = operationId;
            Agent = agent;
        }

        public static BuildMessage GatherData(string id, string agnet = "")
            => new BuildMessage(Ids.GatherData, id, agnet);

        public static BuildMessage NoData(string id)
            => new BuildMessage(Ids.NoData, id, string.Empty);

        public static BuildMessage GenerateLangFiles(string id, string agent)
            => new BuildMessage(Ids.GenerateLangFiles, id, agent);

        public static BuildMessage GenerateCsFiles(string id, string agent)
            => new BuildMessage(Ids.GenerateCsFiles, id, agent);

        public static BuildMessage AgentCompled(string id, string agent)
            => new BuildMessage(Ids.AgentCompled, id, agent);
    }
}