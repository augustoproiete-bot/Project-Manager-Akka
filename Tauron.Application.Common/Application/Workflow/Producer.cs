using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Tauron.Application.Workflow
{
    [PublicAPI]
    public abstract class Producer<TState, TContext>
        where TState : IStep<TContext>

    {
        private readonly Dictionary<StepId, StepRev<TState, TContext>> _states;

        private string _errorMessage = string.Empty;

        private StepId _lastId;

        protected Producer()
        {
            _states = new Dictionary<StepId, StepRev<TState, TContext>>();
        }

        public void Begin(StepId id, [NotNull] TContext context)
        {
            Argument.NotNull(context, nameof(context));

            Process(id, context);

            if (_lastId.Name == StepId.Fail.Name)
                throw new InvalidOperationException(_errorMessage);
        }

        [DebuggerStepThrough]
        protected bool SetLastId(StepId id)
        {
            _lastId = id;
            return _lastId.Name == StepId.Finish.Name || _lastId.Name == StepId.Fail.Name;
        }

        protected virtual bool Process(StepId id, [NotNull] TContext context)
        {
            Argument.NotNull(context, nameof(context));

            if (SetLastId(id)) return true;

            if (!_states.TryGetValue(id, out var rev))
                return SetLastId(StepId.Fail);

            var sId = rev.Step.OnExecute(context);
            var result = false;

            switch (sId.Name)
            {
                case "Fail":
                    _errorMessage = rev.Step.ErrorMessage;
                    return SetLastId(sId);
                case "None":
                    result = ProgressConditions(rev, context);
                    break;
                case "Loop":
                    var ok = true;

                    do
                    {
                        var loopId = rev.Step.NextElement(context);
                        if (loopId.Name == StepId.LoopEnd.Name)
                        {
                            ok = false;
                            continue;
                        }

                        if (loopId.Name == StepId.Fail.Name)
                            return SetLastId(StepId.Fail);

                        ProgressConditions(rev, context);
                    } while (ok);

                    break;
                case "Finish":
                case "Skip":
                    result = true;
                    break;
                default:
                    return SetLastId(StepId.Fail);
            }

            if (!result)
                rev.Step.OnExecuteFinish(context);

            return result;
        }

        private bool ProgressConditions([NotNull] StepRev<TState, TContext> rev, TContext context)
        {
            var std = (from con in rev.Conditions
                let stateId = con.Select(rev.Step, context)
                where stateId.Name != StepId.None.Name
                select stateId).ToArray();

            if (std.Length != 0) return std.Any(id => Process(id, context));

            if (rev.GenericCondition == null) return false;
            var cid = rev.GenericCondition.Select(rev.Step, context);
            return cid.Name != StepId.None.Name && Process(cid, context);
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
        {
            return new StepConfiguration<TState, TContext>(_states[id]);
        }
    }

    [PublicAPI]
    public class StepConfiguration<TState, TContext>
    {
        private readonly StepRev<TState, TContext> _context;

        internal StepConfiguration([NotNull] StepRev<TState, TContext> context)
        {
            _context = context;
        }

        [NotNull]
        public StepConfiguration<TState, TContext> WithCondition([NotNull] ICondition<TContext> condition)
        {
            Argument.NotNull(condition, nameof(condition));

            _context.Conditions.Add(condition);
            return this;
        }

        [NotNull]
        public ConditionConfiguration<TState, TContext> WithCondition(Func<TContext, IStep<TContext>, bool>? guard = null)
        {
            var con = new SimpleCondition<TContext> { Guard = guard };
            if (guard != null) return new ConditionConfiguration<TState, TContext> (WithCondition(con), con);

            _context.GenericCondition = con;
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

        public ICondition<TContext>? GenericCondition { get; set; }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Step);

            foreach (var condition in Conditions) b.AppendLine("->" + condition + ";");

            if (GenericCondition != null) b.Append("Generic->" + GenericCondition + ";");

            return b.ToString();
        }
    }
}