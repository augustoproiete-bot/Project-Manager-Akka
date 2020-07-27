namespace Tauron.Application.Master.Commands.Host
{
    public sealed class OperationResponse
    {
        public bool Success { get; }
        
        public OperationResponse(bool success) 
            => Success = success;
    }
}