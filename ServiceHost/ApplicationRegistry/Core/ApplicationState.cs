using Akkatecture.Aggregates;

namespace ServiceHost.ApplicationRegistry.Core
{
    public class ApplicationState : AggregateState<Application, ApplicationId>,
        IApply<NewApplicationEvent>,
        IApply<NewVersionEvent>
    {
        public ApplicationName Name { get; set; }

        public ApplicationPath Path { get; set; }

        public ApplicationVersion Version { get; set; }


        public void Apply(NewApplicationEvent aggregateEvent)
        {
            Name = aggregateEvent.Name;
            Path = aggregateEvent.Path;
            Version = aggregateEvent.Version;
        }

        public void Apply(NewVersionEvent aggregateEvent) 
            => Version = aggregateEvent.Version;
    }
}