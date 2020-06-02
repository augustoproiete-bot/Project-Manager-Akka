namespace Tauron.Application.Localizer.DataModel.Processing
{
    public abstract class Operation
    {
        protected Operation(string operationId)
        {
            OperationId = operationId;
        }

        public string OperationId { get; }
    }
}