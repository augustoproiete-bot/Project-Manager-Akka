using Akkatecture.Aggregates;
using Akkatecture.Specifications.Provided;

namespace ServiceHost.ApplicationRegistry.Core
{
    [AggregateName(nameof(Application))]
    public sealed class Application : AggregateRoot<Application, ApplicationId, ApplicationState>, 
        IExecute<NewVersionCommand>,
        IExecute<RegisterApplicationCommand>
    {
        public Application(ApplicationId id) 
            : base(id)
        {
        }

        public bool Execute(RegisterApplicationCommand command)
        {
            var isNew = new AggregateIsNewSpecification();
            var path = new ValidPathSpecification();

            if (isNew.IsSatisfiedBy(this) && path.IsSatisfiedBy(command.Path)) 
                Emit(new NewApplicationEvent(command.Name, command.Path, new ApplicationVersion(0)));

            return true;
        }

        public bool Execute(NewVersionCommand command)
        {
            var isNew = new AggregateIsNewSpecification();
            
            if(!isNew.IsSatisfiedBy(this))
                Emit(new NewVersionEvent(State.Name, new ApplicationVersion(State.Version.Value)));

            return true;
        }
    }
}