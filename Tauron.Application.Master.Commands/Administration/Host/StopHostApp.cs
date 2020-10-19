namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class StopHostApp : InternalHostMessages.CommandBase
    {
        public string AppName { get; }


        public StopHostApp(string target, string appName) : base(target, InternalHostMessages.CommandType.AppManager) 
            => AppName = appName;
    }
}