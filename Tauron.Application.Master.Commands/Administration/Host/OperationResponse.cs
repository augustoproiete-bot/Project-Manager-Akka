namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class OperationResponse
    {
        public bool Success { get; private set; }
        
        public OperationResponse(bool success) 
            => Success = success;
    }
}