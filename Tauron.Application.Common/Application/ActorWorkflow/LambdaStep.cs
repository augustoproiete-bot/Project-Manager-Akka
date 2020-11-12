using System;
using JetBrains.Annotations;
using Tauron.Application.Workflow;

namespace Tauron.Application.ActorWorkflow
{
    [PublicAPI]
    public sealed class LambdaStep<TContext> : IStep<TContext>, IHasTimeout
    {
        private readonly Func<TContext, LambdaStep<TContext>, StepId>? _onExecute;
        private readonly Func<TContext, LambdaStep<TContext>, StepId>? _onNextElement;
        private readonly Action<TContext>? _onFinish;

        public string ErrorMessage { get; set; } = string.Empty;
        

        public LambdaStep(Func<TContext, 
                LambdaStep<TContext>, StepId>? onExecute = null, 
            Func<TContext, LambdaStep<TContext>, StepId>? onNextElement = null, 
            Action<TContext>? onFinish = null, TimeSpan? timeout = null)
        {
            Timeout = timeout;
            _onExecute = onExecute;
            _onNextElement = onNextElement;
            _onFinish = onFinish;
        }

        public StepId OnExecute(TContext context) => _onExecute?.Invoke(context, this) ?? StepId.None;

        public StepId NextElement(TContext context) => _onNextElement?.Invoke(context, this) ?? StepId.None;

        public void OnExecuteFinish(TContext context) => _onFinish?.Invoke(context);
        public TimeSpan? Timeout { get; }

        public void SetError(string error)
            => ErrorMessage = error;
    }

    [PublicAPI]
    public sealed class LambdaStepConfiguration<TContext>
    {
        private Func<TContext, LambdaStep<TContext>, StepId>? _onExecute;
        private Func<TContext, LambdaStep<TContext>, StepId>? _onNextElement;
        private Action<TContext>? _onFinish;
        private TimeSpan? _timeout;

        public void OnExecute(Func<TContext, LambdaStep<TContext>, StepId> func) 
            => _onExecute = _onExecute.Combine(func);

        public void OnNextElement(Func<TContext, LambdaStep<TContext>, StepId> func) 
            => _onNextElement = _onNextElement.Combine(func);

        public void OnExecute(Func<TContext, StepId> func) 
            => _onExecute = _onExecute.Combine((c, _) => func(c));

        public void OnNextElement(Func<TContext, StepId> func) 
            => _onNextElement = _onNextElement.Combine((c, _) =>  func(c));

        public void OnFinish(Action<TContext> func) 
            => _onFinish = _onFinish.Combine(func);

        public void WithTimeout(TimeSpan timeout)
            => _timeout = timeout;


        public LambdaStep<TContext> Build() 
            => new(_onExecute, _onNextElement, _onFinish, _timeout);
    }
}