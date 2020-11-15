using System;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Preload;

namespace Tauron.Application.Workflow
{
    [PublicAPI]
    public class SimpleCondition<TContext> : ICondition<TContext>
    {
        public SimpleCondition()
        {
            Target = StepId.None;
        }

        public Maybe<Func<Maybe<TContext>, IStep<TContext>, bool>> Guard { get; set; }

        public StepId Target { get; set; }

        public StepId Select(IStep<TContext> lastStep, Maybe<TContext> context) 
            => OrElse(Match(Guard, g => g(context, lastStep) ? Target : StepId.None, () => May(Target)), StepId.None);

        public override string ToString() 
            => "Target: " + Target;
    }
}