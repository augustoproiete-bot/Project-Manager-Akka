using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.ActorWorkflow
{
    [PublicAPI]
    public sealed record WorkflowResult<TContext>(bool Succesfully, Maybe<string> Error, Maybe<TContext> Context);
}