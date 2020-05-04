using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    [PublicAPI]
    public class RemoteServiceRequest
    {
        public string Name { get; }

        public RemoteServiceRequest(string name) => Name = name;
    }
}