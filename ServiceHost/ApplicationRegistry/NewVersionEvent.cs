using Akkatecture.Aggregates;
using ServiceHost.ApplicationRegistry.Core;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class NewVersionEvent : AggregateEvent<Application, ApplicationId>
    {
        public ApplicationName Name { get; }

        public ApplicationVersion Version { get; }

        public NewVersionEvent(ApplicationName name, ApplicationVersion version)
        {
            Name = name;
            Version = version;
        }
    }
}