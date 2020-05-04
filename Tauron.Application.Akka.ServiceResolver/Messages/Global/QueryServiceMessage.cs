using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages.Global
{
    [PublicAPI]
    public class QueryServiceRequest
    {
        public string Name { get; }

        public QueryServiceRequest(string name) => Name = name;
    }
}