using Akka.Actor;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    public class HostLocalServiceMessage
    {
        public Props Props { get; }

        public string Name { get; }

        public HostLocalServiceMessage(Props props, string name)
        {
            Props = props;
            Name = name;
        }
    }
}