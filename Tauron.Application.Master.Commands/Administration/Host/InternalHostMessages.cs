using Newtonsoft.Json;

namespace Tauron.Application.Master.Commands.Administration.Host
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

            [JsonIgnore]
            public CommandType Type { get; }

            protected CommandBase(string target, CommandType type)
            {
                Target = target;
                Type = type;
            }
        }

        public sealed class GetHostName
        { }

        public sealed class GetHostNameResult
        {
            public string Name { get; }

            public GetHostNameResult(string name) => Name = name;
        }
    }
}