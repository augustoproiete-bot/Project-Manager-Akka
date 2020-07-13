using Akkatecture.Aggregates;
using ServiceHost.ApplicationRegistry.Core;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class NewApplicationEvent : AggregateEvent<Application, ApplicationId>
    {
        public ApplicationName Name { get; }

        public ApplicationPath Path { get; }

        public ApplicationVersion Version { get; }

        public NewApplicationEvent(ApplicationName name, ApplicationPath path, ApplicationVersion version)
        {
            Name = name;
            Path = path;
            Version = version;
        }
    }
}