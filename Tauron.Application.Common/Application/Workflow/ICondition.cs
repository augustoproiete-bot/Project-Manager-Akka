using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron.Application.Workflow
{
    public interface ICondition<TContext>
    {
        StepId Select(IStep<TContext> lastStep, Maybe<TContext> context);
    }
}