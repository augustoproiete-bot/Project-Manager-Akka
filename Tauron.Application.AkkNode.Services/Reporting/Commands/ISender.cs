namespace Tauron.Application.AkkNode.Services.Commands
{
    public interface ISender
    {
        void SendCommand(IReporterMessage command);
    }
}