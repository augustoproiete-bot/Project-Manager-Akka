namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class BuildMessage
    {
        public static class Ids
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            public const string GatherData = nameof(GatherData);

            public const string NoData = nameof(NoData);
            // ReSharper restore MemberHidesStaticFromOuterClass
        }

        public string Message { get; }

        public string OperationId { get; }

        public BuildMessage(string message, string operationId)
        {
            Message = message;
            OperationId = operationId;
        }

        public static BuildMessage GatherData(string id)
            => new BuildMessage(Ids.GatherData, id);

        public static BuildMessage NoData(string id)
            => new BuildMessage(Ids.NoData, id);
    }
}