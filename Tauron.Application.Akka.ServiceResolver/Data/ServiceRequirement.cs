using System.Collections.Generic;

namespace Tauron.Application.Akka.ServiceResolver.Data
{
    public sealed class ServiceRequirement
    {
        public IReadOnlyList<string> NeededServices { get; }

        public ServiceRequirement(IReadOnlyList<string> neededServices) 
            => NeededServices = neededServices;
    }
}