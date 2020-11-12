using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Application.Workflow;

namespace Tauron.Application.ActorWorkflow
{
    [PublicAPI]
    public abstract class LambdaWorkflowActor<TContext> : WorkflowActorBase<LambdaStep<TContext>, TContext>
        where TContext : IWorkflowContext
    {
        protected void WhenStep(StepId id, Action<LambdaStepConfiguration<TContext>> config, Action<StepConfiguration<LambdaStep<TContext>, TContext>>? con = null)
        {
            var stepConfig = new LambdaStepConfiguration<TContext>();
            config.Invoke(stepConfig);
            var concon = WhenStep(id, stepConfig.Build());
            con?.Invoke(concon);
        }
    }

    [PublicAPI]
    public abstract class WorkflowActorBase<TStep, TContext> : ActorBase, IWithTimers
        where TStep : IStep<TContext> where TContext : IWorkflowContext
    {
        protected ILoggingAdapter Log { get; } = Context.GetLogger();
        protected TContext RunContext { get; private set; } = default!;

        private readonly Dictionary<StepId, StepRev<TStep, TContext>> _steps = new();
        private readonly Dictionary<Type, Delegate> _starter = new();
        private readonly Dictionary<Type, Delegate> _signals = new();
        private readonly object _timeout = new();

        private Action<WorkflowResult<TContext>>? _onFinish;
        
        private bool _running;
        private bool _waiting;
        private ChainCall? _lastCall;
        private IActorRef? _starterSender;

        private string _errorMessage = string.Empty;

        protected void SetError(string error)
            => _errorMessage = error;

        protected override bool Receive(object message)
        {
            if (_running)
                return _waiting ? Singnaling(message) : Running(message);
            return Initializing(message);
        }

        protected virtual bool Singnaling(object msg)
        {
            try
            {
                if (msg is TimeoutMarker)
                {
                    _errorMessage = "Timeout";
                    Finish(false);
                    return true;
                }

                if (!_signals.TryGetValue(msg.GetType(), out var del)) return false;
                Timers.Cancel(_timeout);

                if (del.DynamicInvoke(RunContext, msg) is not StepId id)
                    throw new InvalidOperationException("Invalid Call of Signal Delegate");

                Self.Tell(new ChainCall(id).WithBase(_lastCall), _starterSender);

                _lastCall = null;
                return true;
            }
            finally
            {
                _waiting = false;
            }
        }

        protected virtual bool Initializing(object msg)
        {
            switch (msg)
            {
                case ChainCall:
                case LoopElement:
                    return true;
                case WorkflowResult<TContext> result:
                    _onFinish?.Invoke(result);
                    return true;
            }

            if (!_starter.TryGetValue(msg.GetType(), out var del)) return false;
            del.DynamicInvoke(msg);
            return true;

        }

        protected void Signal<TMessage>(Func<TContext, TMessage, StepId> signal)
            => _signals[typeof(TMessage)] = signal;

        protected void StartMessage<TType>(Action<TType> msg) 
            => _starter[typeof(TType)] = msg;

        protected virtual bool Running(object msg)
        {
            try
            {
                switch (msg)
                {
                    case ChainCall chain:
                    {
                        var id = chain.Id;
                        if (id == StepId.Fail)
                        {
                            Finish(false);
                            return true;
                        }

                        if (!_steps.TryGetValue(id, out var rev))
                        {
                            Log.Warning("No Step Found {Id}", id.Name);
                            _errorMessage = id.Name;
                            Finish(false);
                            return true;
                        }

                        var sId = rev.Step.OnExecute(RunContext);

                        switch (sId.Name)
                        {
                            case "Fail":
                                _errorMessage = rev.Step.ErrorMessage;
                                Finish(false);
                                break;
                            case "None":
                                ProgressConditions(rev, true, chain);
                                return true;
                            case "Loop":
                                Self.Forward(new LoopElement(rev, chain));
                                return true;
                            case "Finish":
                            case "Skip":
                                Finish(true, rev);
                                break;
                            case "Waiting":
                                _waiting = true;
                                if (rev.Step is IHasTimeout timeout && timeout.Timeout != null) 
                                    Timers.StartSingleTimer(_timeout, new TimeoutMarker(), timeout.Timeout.Value);
                                _lastCall = chain;
                                return true;
                            default:
                                Self.Forward(new ChainCall(sId).WithBase(chain));
                                return true;
                        }
                        if(_running)
                            Self.Forward(chain.Next());

                        return true;
                    }
                    case LoopElement loop:
                    {
                        var loopId = loop.Rev.Step.NextElement(RunContext);
                        if (loopId != StepId.LoopEnd) 
                            Self.Forward(new LoopElement(loop.Rev, loop.Call));

                        if (loopId == StepId.LoopContinue)
                            return true;

                        if (loopId.Name == StepId.Fail.Name)
                        {
                            Finish(false);
                            return true;
                        }

                        ProgressConditions(loop.Rev, baseCall:loop.Call);
                        return true;
                    }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception While Processing Workflow");
                _errorMessage = e.Message;
                Finish(false);
                return true;
            }
        }

        private void ProgressConditions(StepRev<TStep, TContext> rev, bool finish = false, ChainCall? baseCall = null)
        {
            var std = (from con in rev.Conditions
                let stateId = con.Select(rev.Step, RunContext)
                where stateId.Name != StepId.None.Name
                select stateId).ToArray();

            if (std.Length != 0)
            {
                Self.Forward(new ChainCall(std).WithBase(baseCall));
                return;
            }

            if (rev.GenericCondition == null)
            {
                if(finish)
                    Finish(false);
            }
            else
            {
                var cid = rev.GenericCondition.Select(rev.Step, RunContext);
                if(cid.Name != StepId.None.Name)
                    Self.Forward(new ChainCall(cid).WithBase(baseCall));
            }
        }

        protected void OnFinish(Action<WorkflowResult<TContext>> con) 
            => _onFinish = _onFinish.Combine(con);

        private void Finish(bool isok, StepRev<TStep, TContext>? rev = null)
        {
            _starterSender = null;
            _running = false;
            if(isok)
                rev?.Step.OnExecuteFinish(RunContext);
            Self.Forward(new WorkflowResult<TContext>(isok, _errorMessage, RunContext));
            RunContext = default!;
            _errorMessage = string.Empty;
        }

        public void Start(TContext context)
        {
            _starterSender = Sender;
            _running = true;
            RunContext = context;
            Self.Forward(new ChainCall(StepId.Start));
        }

        protected StepConfiguration<TStep, TContext> WhenStep(StepId id, TStep step)
        {
            var rev = new StepRev<TStep, TContext>(step);
            _steps[id] = rev;
            return new StepConfiguration<TStep, TContext>(rev);
        }

        private sealed class TimeoutMarker
        {
            
        }

        private sealed class LoopElement
        {
            public StepRev<TStep, TContext> Rev { get; }
            public ChainCall Call { get; }

            public LoopElement(StepRev<TStep, TContext> rev, ChainCall call)
            {
                Rev = rev;
                Call = call;
            }
        }

        private sealed class ChainCall
        {
            private ChainCall? BaseCall { get; }

            private StepId[] StepIds { get; }

            private int Position { get; }

            private ChainCall(StepId[] stepIds, int position, ChainCall? baseCall = null)
            {
                BaseCall = baseCall;
                StepIds = stepIds;
                Position = position;
            }

            public ChainCall(StepId id, ChainCall? baseCall = null)
            {
                BaseCall = baseCall;
                StepIds = new[] { id };
                Position = 0;
            }

            public ChainCall(StepId[] ids)
            {
                StepIds = ids;
                Position = 0;
            }

            public StepId Id => Position >= StepIds.Length ? BaseCall?.Id ?? StepId.Fail : StepIds[Position];

            public ChainCall Next()
            {
                var newPos = Position + 1;
                if (newPos == StepIds.Length && BaseCall != null) return BaseCall.Next();
                return new ChainCall(StepIds, newPos);
            }

            public ChainCall WithBase(ChainCall? call)
            {
                if (call == null)
                    return this;
                else
                {
                    call = call.Next();
                    return new ChainCall(call.StepIds, call.Position, this);
                }
            }
        }

        public ITimerScheduler Timers { get; set; } = default!;
    }
}