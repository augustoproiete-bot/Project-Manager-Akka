namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class BuildCompled
    {
        public string OperationId { get; }

        public bool Failed { get; }

        public BuildCompled(string operationId, bool failed)
        {
            OperationId = operationId;
            Failed = failed;
        }
    }
}