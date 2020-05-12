namespace Tauron.Application.Localizer.DataModel.Processing
{
    public abstract class Operation
    {
        public string OperationId { get; }

        protected Operation(string operationId) => OperationId = operationId;
    }
}