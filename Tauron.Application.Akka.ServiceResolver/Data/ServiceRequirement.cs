using System;
using System.Collections.Generic;
using System.Linq;

namespace Tauron.Application.Akka.ServiceResolver.Data
{
    public sealed class ServiceRequirement
    {
        public static readonly ServiceRequirement Empty = new ServiceRequirement(Array.Empty<string>());

        public ServiceRequirement(IReadOnlyList<string> neededServices)
        {
            NeededServices = neededServices;
        }

        public IReadOnlyList<string> NeededServices { get; }

        public static ServiceRequirement Create(params string[] neededServices)
        {
            return new ServiceRequirement(neededServices);
        }

        public bool IsDefiend(IReadOnlyList<string> services)
        {
            return NeededServices.All(services.Contains!);
        }
    }
}