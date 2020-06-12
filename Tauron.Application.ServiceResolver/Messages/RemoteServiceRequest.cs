using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    [PublicAPI]
    public class RemoteServiceRequest
    {
        public RemoteServiceRequest(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}