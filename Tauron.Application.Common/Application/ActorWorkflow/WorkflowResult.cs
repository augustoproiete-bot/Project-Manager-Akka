using JetBrains.Annotations;

namespace Tauron.Application.ActorWorkflow
{
    [PublicAPI]
    public sealed record WorkflowResult<TContext>(bool Succesfully, string Error, TContext Context);
}