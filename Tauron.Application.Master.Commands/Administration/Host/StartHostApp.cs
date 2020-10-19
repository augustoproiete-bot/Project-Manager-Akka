namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class StartHostApp : InternalHostMessages.CommandBase
    {
        public string AppName { get; }


        public StartHostApp(string target, string appName) : base(target, InternalHostMessages.CommandType.AppManager) 
            => AppName = appName;
    }
}