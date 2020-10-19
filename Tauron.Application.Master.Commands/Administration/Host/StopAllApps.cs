namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class StopAllApps : InternalHostMessages.CommandBase
    {
        public StopAllApps(string target) 
            : base(target, InternalHostMessages.CommandType.AppManager)
        {
        }
    }
}