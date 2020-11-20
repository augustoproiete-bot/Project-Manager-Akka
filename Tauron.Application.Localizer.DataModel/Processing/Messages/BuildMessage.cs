namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record BuildMessage(string Message, string OperationId, string Agent)
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

        public static BuildMessage GatherData(string id, string agnet = "")
            => new(Ids.GatherData, id, agnet);

        public static BuildMessage NoData(string id)
            => new(Ids.NoData, id, string.Empty);

        public static BuildMessage GenerateLangFiles(string id, string agent)
            => new(Ids.GenerateLangFiles, id, agent);

        public static BuildMessage GenerateCsFiles(string id, string agent)
            => new(Ids.GenerateCsFiles, id, agent);

        public static BuildMessage AgentCompled(string id, string agent)
            => new(Ids.AgentCompled, id, agent);
    }
}