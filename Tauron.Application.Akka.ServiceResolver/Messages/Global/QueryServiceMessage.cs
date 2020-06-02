using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages.Global
{
    [PublicAPI]
    public class QueryServiceRequest
    {
        public QueryServiceRequest(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}