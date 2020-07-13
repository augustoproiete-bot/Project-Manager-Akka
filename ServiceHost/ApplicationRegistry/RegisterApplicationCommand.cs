using Akkatecture.Commands;
using ServiceHost.ApplicationRegistry.Core;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class RegisterApplicationCommand : Command<Application, ApplicationId>
    {
        public ApplicationName Name { get; }

        public ApplicationPath Path { get; }

        public RegisterApplicationCommand(ApplicationId aggregateId, ApplicationName name, ApplicationPath path) : base(aggregateId)
        {
            Name = name;
            Path = path;
        }

        public RegisterApplicationCommand(ApplicationId aggregateId, CommandId sourceId, ApplicationName name, ApplicationPath path) : base(aggregateId, sourceId)
        {
            Name = name;
            Path = path;
        }
    }
}