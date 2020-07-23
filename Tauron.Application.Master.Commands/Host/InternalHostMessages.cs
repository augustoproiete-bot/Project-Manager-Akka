namespace Tauron.Application.Master.Commands.Host
{
    public static class InternalHostMessages
    {
        public enum CommandType
        {
            AppManager,
            AppRegistry,
            Installer
        }

        public abstract class CommandBase
        {
            public string Target { get; }

            public CommandType Type { get; }

            protected CommandBase(string target, CommandType type)
            {
                Target = target;
                Type = type;
            }
        }

        public sealed class GetHostName { }

        public sealed class GetHostNameResult
        {
            public string Name { get; }

            public GetHostNameResult(string name) => Name = name;
        }
    }
}