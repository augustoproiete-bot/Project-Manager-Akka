using System;
using System.Collections.Generic;
using System.Linq;

namespace Tauron.Application.Akka.ServiceResolver.Data
{
    public sealed class ServiceRequirement
    {
        public static readonly ServiceRequirement Empty = new ServiceRequirement(Array.Empty<string>()); 

        public IReadOnlyList<string> NeededServices { get; }

        public ServiceRequirement(IReadOnlyList<string> neededServices) 
            => NeededServices = neededServices;

        public bool IsDefiend(IReadOnlyList<string> services) 
            => NeededServices.All(services.Contains!);
    }
}