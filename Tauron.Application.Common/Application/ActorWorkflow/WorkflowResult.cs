namespace Tauron.Application.ActorWorkflow
{
    public sealed class WorkflowResult<TContext>
    {
        public bool Succesfully { get; }

        public string Error { get; }

        public TContext Context { get; }

        public WorkflowResult(bool succesfully, string error, TContext context)
        {
            Succesfully = succesfully;
            Error = error;
            Context = context;
        }
    }
}