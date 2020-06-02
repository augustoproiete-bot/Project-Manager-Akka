using Akka.Actor;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    public class HostLocalServiceMessage
    {
        public HostLocalServiceMessage(Props props, string name)
        {
            Props = props;
            Name = name;
        }

        public Props Props { get; }

        public string Name { get; }
    }
}