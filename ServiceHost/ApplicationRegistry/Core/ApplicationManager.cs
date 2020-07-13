using Akkatecture.Aggregates;
using Akkatecture.Commands;

namespace ServiceHost.ApplicationRegistry.Core
{
    public sealed class ApplicationManager : AggregateManager<Application, ApplicationId, Command<Application, ApplicationId>>
    {
        
    }
}