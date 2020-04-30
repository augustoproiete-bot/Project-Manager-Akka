using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Akka.ServiceResolver.Data;

namespace Tauron.Application.Akka.ServiceResolver.Messages.Global
{
    public sealed class RegisterEndpointMessage
    {
        public ServiceRequirement Requirement { get; }

        public IReadOnlyList<string> ProvidedServices { get; }

        public RegisterEndpointMessage(ServiceRequirement requirement, IReadOnlyList<string> providedServices)
        {
            if(requirement.NeededServices.Any(providedServices.Contains!))
                throw new ArgumentException("Provided Services Cannot ben Needed", nameof(requirement));

            Requirement = requirement;
            ProvidedServices = providedServices;
        }
    }
}