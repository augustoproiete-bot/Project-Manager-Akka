using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron.Application.Workflow
{
    [PublicAPI]
    public abstract class Producer<TState, TContext>
        where TState : IStep<TContext>

    {
        private readonly Dictionary<StepId, StepRev<TState, TContext>> _states;

        private Maybe<string> _errorMessage;

        private Maybe<StepId> _lastId;

        protected Producer() 
            => _states = new Dictionary<StepId, StepRev<TState, TContext>>();

        public void Begin(StepId id, Maybe<TContext> context)
        {
            StepProcess(May(id), context).OrElse(() => new InvalidOperationException("Process Result not set"));

            Do(from lastId in _lastId
               where lastId == StepId.Fail
               select Action(() => throw new InvalidOperationException(_errorMessage.OrElse("Unkowen Error"))));
        }

        [DebuggerStepThrough]
        protected Maybe<bool> SetLastId(Maybe<StepId> id)
        {
            _lastId = id;
            return from lastId in _lastId
                   select lastId == StepId.Finish || lastId == StepId.Fail;
        }

        protected virtual Maybe<bool> StepProcess(Maybe<StepId> mayId, Maybe<TContext> context)
        {
            Maybe<bool> ProcessId(StepId sId, StepRev<TState, TContext> rev, Maybe<TContext> context)
            {
                var result = May(false);

                switch (sId.Name)
                {
                    case "Fail":
                        _errorMessage = rev.Step.ErrorMessage;
                        return SetLastId(May(sId));
                    case "None":
                        result = ProgressConditions(rev, context);
                        break;
                    case "Loop":
                        // ReSharper disable once RedundantAssignment
                        var mayOk = May(true);

                        do
                        {
                            var mayLoopId = rev.Step.NextElement(context);

                            mayOk = Either(from loopId in mayLoopId
                                       where loopId == StepId.LoopEnd
                                       select false, true);

                            var fail = from loopId in mayLoopId
                                       where loopId == StepId.Fail
                                       select SetLastId(May(StepId.Fail));

                            if (fail.IsSomething())
                                return fail;

                            ProgressConditions(rev, context);
                        } while (mayOk.OrElse(false));

                        break;
                    case "Finish":
                    case "Skip":
                        result = May(true);
                        break;
                    default:
                        return SetLastId(May(StepId.Fail));
                }

                Do(from res in result
                   where res
                   select Action(() => rev.Step.OnExecuteFinish(context)));

                return result;
            }

            return from id in mayId
                   from lastIdResult in SetLastId(mayId)
                   where lastIdResult
                   from rev in _states.Lookup(id)
                   from newId in rev.Step.OnExecute(context)
                   select ProcessId(newId, rev, context);
        }

        private Maybe<bool> ProgressConditions(StepRev<TState, TContext> rev, Maybe<TContext> context)
        {
            var std = (from con in rev.Conditions
                let stateId = con.Select(rev.Step, context)
                where stateId.Name != StepId.None.Name
                select stateId).ToArray();

            if (std.Length != 0) return Any(std.Select(id => new Func<Maybe<bool>>(() => StepProcess(May(id), context))));

            if (rev.GenericCondition.IsNothing()) return May(false);

            return from condition in rev.GenericCondition
                   from newId in May(condition.Select(rev.Step, context))
                   where newId != StepId.None
                   select StepProcess(May(newId), context);
        }

        [NotNull]
        public StepConfiguration<TState, TContext> SetStep(StepId id, TState stade)
        {
            Argument.NotNull(stade, nameof(stade));

            var rev = new StepRev<TState, TContext>(stade);
            _states[id] = rev;

            return new StepConfiguration<TState, TContext>(rev);
        }

        [NotNull]
        public StepConfiguration<TState, TContext> GetStateConfiguration(StepId id) 
            => new(_states[id]);
    }

    [PublicAPI]
    public class StepConfiguration<TState, TContext>
    {
        private readonly StepRev<TState, TContext> _context;

        internal StepConfiguration([NotNull] StepRev<TState, TContext> context) 
            => _context = context;

        [NotNull]
        public StepConfiguration<TState, TContext> WithCondition([NotNull] ICondition<TContext> condition)
        {
            Argument.NotNull(condition, nameof(condition));

            _context.Conditions.Add(condition);
            return this;
        }

        [NotNull]
        public ConditionConfiguration<TState, TContext> WithCondition(Maybe<Func<Maybe<TContext>, IStep<TContext>, bool>> guard = default)
        {
            var con = new SimpleCondition<TContext> { Guard = guard };
            if (guard.IsSomething()) return new ConditionConfiguration<TState, TContext> (WithCondition(con), con);

            _context.GenericCondition = May<ICondition<TContext>>(con);
            return new ConditionConfiguration<TState, TContext>(this, con);
        }
    }

    [PublicAPI]
    public class ConditionConfiguration<TState, TContext>
    {
        private readonly SimpleCondition<TContext> _condition;
        private readonly StepConfiguration<TState, TContext> _config;

        public ConditionConfiguration([NotNull] StepConfiguration<TState, TContext> config, [NotNull] SimpleCondition<TContext> condition)
        {
            Argument.NotNull(config, nameof(config));
            Argument.NotNull(condition, nameof(condition));

            _config = config;
            _condition = condition;
        }

        [NotNull]
        public StepConfiguration<TState, TContext> GoesTo(StepId id)
        {
            _condition.Target = id;

            return _config;
        }
    }

    internal class StepRev<TState, TContext>
    {
        public StepRev(TState step)
        {
            Step = step;
            Conditions = new List<ICondition<TContext>>();
        }

        public TState Step { get; }

        [NotNull] public List<ICondition<TContext>> Conditions { get; }

        public Maybe<ICondition<TContext>> GenericCondition { get; set; }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Step);

            foreach (var condition in Conditions) b.AppendLine("->" + condition + ";");

            if (GenericCondition.IsSomething()) b.Append("Generic->" + GenericCondition.Value + ";");

            return b.ToString();
        }
    }
}