using System;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workflow;
using static Tauron.Prelude;

namespace Tauron.Application.ActorWorkflow
{
    public delegate Maybe<StepId> LambdaExecution<TContext>(LambdaStep<TContext> step, Maybe<TContext> mayContext);

    public delegate void LambdaFinish<TContext>(Maybe<TContext> mayContext);

    [PublicAPI]
    public sealed class LambdaStep<TContext> : IStep<TContext>, IHasTimeout
    {
        private readonly Maybe<LambdaExecution<TContext>> _onExecute;
        private readonly Maybe<LambdaExecution<TContext>> _onNextElement;
        private readonly Maybe<LambdaFinish<TContext>> _onFinish;

        public Maybe<string> ErrorMessage { get; set; }
        

        public LambdaStep(
                Maybe<LambdaExecution<TContext>> onExecute = default, 
                Maybe<LambdaExecution<TContext>> onNextElement = default, 
                Maybe<LambdaFinish<TContext>> onFinish = default, Maybe<TimeSpan> timeout = default)
        {
            Timeout = timeout;
            _onExecute = onExecute;
            _onNextElement = onNextElement;
            _onFinish = onFinish;
        }

        public Maybe<StepId> OnExecute(Maybe<TContext> context)
        {
            return Collapse(from exec in _onExecute
                            select exec(this, context))
               .Or(May(StepId.None));
        }

        public Maybe<StepId> NextElement(Maybe<TContext> context)
        {
            return Collapse(from exec in _onNextElement
                            select exec(this, context))
               .Or(May(StepId.None));
        }

        public void OnExecuteFinish(Maybe<TContext> context)
        {
            Do(from finish in _onFinish 
               select Use(() => finish(context)));
        }

        public Maybe<TimeSpan> Timeout { get; }

        public void SetError(Maybe<string> error)
            => ErrorMessage = error;
    }

    [PublicAPI]
    public sealed class LambdaStepConfiguration<TContext>
    {
        private Maybe<LambdaExecution<TContext>> _onExecute;
        private Maybe<LambdaExecution<TContext>> _onNextElement;
        private Maybe<LambdaFinish<TContext>> _onFinish;
        private Maybe<TimeSpan> _timeout;

        public void OnExecute(Maybe<LambdaExecution<TContext>> func) 
            => _onExecute = _onExecute.Combine(func);

        public void OnNextElement(Maybe<LambdaExecution<TContext>> func) 
            => _onNextElement = _onNextElement.Combine(func);

        public void OnExecute(Maybe<Func<Maybe<TContext>, Maybe<StepId>>> mayFunc)
            => OnExecute(mayFunc.Select(func => new LambdaExecution<TContext>((_, context) => func(context))));

        public void OnNextElement(Maybe<Func<Maybe<TContext>, Maybe<StepId>>> mayFunc)
            => OnNextElement(mayFunc.Select(func => new LambdaExecution<TContext>((_, context) => func(context))));

        public void OnFinish(Maybe<LambdaFinish<TContext>> func) 
            => _onFinish = _onFinish.Combine(func);

        public void WithTimeout(Maybe<TimeSpan> timeout)
            => _timeout = timeout;


        public LambdaStep<TContext> Build() 
            => new(_onExecute, _onNextElement, _onFinish, _timeout);
    }
}