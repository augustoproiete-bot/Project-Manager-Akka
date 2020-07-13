using System.Collections.Generic;
using Akkatecture.Specifications;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class VersionIncrementSpecification : Specification<(ApplicationVersion OldVersion, ApplicationVersion NewVersion)>
    {
        protected override IEnumerable<string> IsNotSatisfiedBecause((ApplicationVersion OldVersion, ApplicationVersion NewVersion) aggregate)
        {
            var (oldVersion, NewVersion) = aggregate;

            if (oldVersion.Value >= NewVersion.Value)
                yield return ApplicationErrorCodes.InvalidVersion;
        }
    }
}