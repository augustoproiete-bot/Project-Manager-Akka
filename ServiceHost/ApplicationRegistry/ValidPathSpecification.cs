using System.Collections.Generic;
using System.IO;
using Akkatecture.Specifications;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class ValidPathSpecification : Specification<ApplicationPath>
    {
        protected override IEnumerable<string> IsNotSatisfiedBecause(ApplicationPath aggregate)
        {
            if (!Directory.Exists(aggregate.Value))
                yield return ApplicationErrorCodes.InvalidPath;
        }
    }
}