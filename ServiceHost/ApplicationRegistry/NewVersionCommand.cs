using Akkatecture.Commands;
using ServiceHost.ApplicationRegistry.Core;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class NewVersionCommand : Command<Application, ApplicationId>
    {
        public NewVersionCommand(ApplicationId aggregateId) : base(aggregateId)
        {
        }

        public NewVersionCommand(ApplicationId aggregateId, CommandId sourceId) : base(aggregateId, sourceId)
        {
        }
    }
}