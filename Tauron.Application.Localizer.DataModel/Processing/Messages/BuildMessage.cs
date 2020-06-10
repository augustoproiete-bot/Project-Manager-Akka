namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class BuildMessage
    {
        public static class Ids
        {
            
        }

        public string Message { get; }

        public string OperationId { get; }

        public BuildMessage(string message, string operationId)
        {
            Message = message;
            OperationId = operationId;
        }
    }
}