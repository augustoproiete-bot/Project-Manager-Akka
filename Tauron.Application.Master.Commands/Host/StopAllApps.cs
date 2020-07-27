namespace Tauron.Application.Master.Commands.Host
{
    public sealed class StopAllApps : InternalHostMessages.CommandBase
    {
        public StopAllApps(string target) 
            : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }
    }
}