using Functional.Maybe;

namespace Tauron.Application.Workflow
{
    public interface IStep<TContext>
    {
        Maybe<string> ErrorMessage { get; }

        //StepId Id { get; }

        Maybe<StepId> OnExecute(Maybe<TContext> context);

        Maybe<StepId> NextElement(Maybe<TContext> context);

        void OnExecuteFinish(Maybe<TContext> context);
    }
}