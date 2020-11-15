using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Application.Workflow;
using static Tauron.Preload;

namespace Tauron.Application.ActorWorkflow
{
    [PublicAPI]
    public abstract class LambdaWorkflowActor<TContext> : WorkflowActorBase<LambdaStep<TContext>, TContext>
        where TContext : IWorkflowContext
    {
        protected void WhenStep(StepId id, Action<LambdaStepConfiguration<TContext>> config, Maybe<Action<StepConfiguration<LambdaStep<TContext>, TContext>>> mayStepConfig = default)
        {
            var stepConfig = new LambdaStepConfiguration<TContext>();
            config.Invoke(stepConfig);
            var stepConfiguration = WhenStep(id, stepConfig.Build());

            Do(from postStepConfig in mayStepConfig
               select Use(() => postStepConfig.Invoke(stepConfiguration)));
        }
    }

    [PublicAPI]
    public abstract class WorkflowActorBase<TStep, TContext> : ActorBase, IWithTimers
        where TStep : IStep<TContext> where TContext : IWorkflowContext
    {
        private enum RunState
        {
            Stoped,
            Running,
            Waiting
        }

        public ITimerScheduler Timers { get; set; } = default!;

        protected ILoggingAdapter Log { get; } = Context.GetLogger();
        protected Maybe<TContext> RunContext { get; private set; }

        private readonly Dictionary<StepId, StepRev<TStep, TContext>> _steps = new();
        private readonly Dictionary<Type, Delegate> _starter = new();
        private readonly Dictionary<Type, Delegate> _signals = new();
        private readonly object _timeout = new();

        private Maybe<Action<WorkflowResult<TContext>>> _onFinish;
        
        private Maybe<RunState> _runState = May(RunState.Stoped);
        private Maybe<ChainCall> _lastCall;
        private Maybe<IActorRef> _starterSender;

        private Maybe<string> _errorMessage;

        protected void SetError(Maybe<string> error)
            => _errorMessage = error;

        protected override bool Receive(object message)
        {
            return OrElse(from run in _runState
                          select run switch
                          {
                              RunState.Stoped => Initializing(message),
                              RunState.Running => Running(message),
                              RunState.Waiting => Singnaling(message),
                              _ => Initializing(message)
                          }, false);
        }

        protected virtual Maybe<bool> Singnaling(object msg)
        {
            Maybe<StepId> TryCall(Delegate del)
                => MayThrow(from id in May(del.DynamicInvoke(RunContext, msg))
                            where id is StepId
                            select (StepId) id,
                    () => new InvalidOperationException("Invalid Call of Signal Delegate"));

            return Finally(() =>
                {
                    switch (msg)
                    {
                        case TimeoutMarker:
                            _errorMessage = May("Timeout");
                            Finish(false);
                            return May(true);
                        default:
                            var callResult =
                                from msgKey in May(msg.GetType())
                                from signal in _signals.Lookup(msgKey)
                                let _ = Use(() => Timers.Cancel(_timeout))
                                select TryCall(signal);

                            return Match(callResult,
                                id =>
                                {
                                    Self.Tell(new ChainCall(id).WithBase(_lastCall), OrElse(_starterSender, ActorRefs.NoSender));
                                    _lastCall = Maybe<ChainCall>.Nothing;
                                    return true;
                                },
                                () => May(false));
                    }
                },
                () => _runState = May(RunState.Running));
        }

        protected virtual Maybe<bool> Initializing(object msg)
        {
            var defaultCall = msg switch
            {
                ChainCall => May(true),
                LoopElement => May(true),
                WorkflowResult<TContext> result => Or(from finish in _onFinish
                                                      select Use(() =>
                                                      {
                                                          finish.Invoke(result);
                                                          return true;
                                                      }), May(true)),
                _ => Maybe<bool>.Nothing
            };

            return Or(defaultCall, from del in _starter.Lookup(msg.GetType())
                                   select from call in May(del.DynamicInvoke(msg))
                                          select true);
        }

        protected void Signal<TMessage>(Func<Maybe<TContext>, TMessage, StepId> signal)
            => _signals[typeof(TMessage)] = signal;

        protected void StartMessage<TType>(Action<TType> msg) 
            => _starter[typeof(TType)] = msg;

        protected virtual Maybe<bool> Running(object msg)
        {
            bool ProcessStep(StepId sId, StepRev<TStep, TContext> stepRev, ChainCall chain)
            {
                switch (sId.Name)
                {
                    case "Fail":
                        _errorMessage = stepRev.Step.ErrorMessage;
                        Finish(false);
                        break;
                    case "None":
                        ProgressConditions(stepRev, true, May(chain));
                        break;
                    case "Loop":
                        Self.Forward(new LoopElement(stepRev, chain));
                        break;
                    case "Finish":
                    case "Skip":
                        Finish(true, May(stepRev));
                        break;
                    case "Waiting":
                        _runState = May(RunState.Waiting);
                        // ReSharper disable once MergeSequentialPatterns
                        if (stepRev.Step is IHasTimeout timeout && timeout.Timeout != null)
                            Timers.StartSingleTimer(_timeout, new TimeoutMarker(), timeout.Timeout.Value);
                        _lastCall = May(chain);
                        break;
                    default:
                        Self.Forward(new ChainCall(sId).WithBase(May(chain)));
                        break;
                }

                return true;
            }

            var result =
                Try(() =>
                {
                    switch (msg)
                    {
                        case ChainCall chain:
                        {
                            var mayId = chain.Id;

                            return Any(
                                () =>
                                    from id in mayId
                                    where id == StepId.Fail
                                    select Finish(false),

                                () =>
                                    RunWith(() =>
                                            from id in mayId
                                            from stepRev in _steps.Lookup(id)
                                            from sId in stepRev.Step.OnExecute(RunContext)
                                            select ProcessStep(sId, stepRev, chain),
                                        () => Do(from state in _runState
                                                 where state == RunState.Running
                                                 select Use(() => Self.Forward(chain.Next())))),

                                () =>
                                    from id in mayId
                                    select Use(() =>
                                    {
                                        Log.Warning("No Step Found {Id}", id.Name);
                                        _errorMessage = May(id.Name);

                                        return Finish(false);
                                    })
                            );
                        }

                        case LoopElement loop:
                        {
                            var loopId = loop.Rev.Step.NextElement(RunContext);

                            return Any(
                                () => from id in loopId
                                      where id == StepId.LoopContinue
                                      select SelfForeward(new LoopElement(loop.Rev, loop.Call)),
                                () => from id in loopId
                                      where id == StepId.LoopEnd
                                      select SelfForeward(loop.Call.Next()),
                                () => from id in loopId
                                      where id == StepId.Fail
                                      select RunWith(() => Finish(false), () => _errorMessage = loop.Rev.Step.ErrorMessage),
                                () => from id in loopId
                                      select OrElse(ProgressConditions(loop.Rev, baseCall: May(loop.Call)), false)
                            );
                        }
                        default:
                            return May(false);
                    }
                });

            return result.Match(m => m,
                e =>
                {
                    Log.Error(e, "Exception While Processing Workflow");
                    _errorMessage = May(e.Message);
                    Finish(false);
                    return May(true);
                });
        }

        private bool SelfForeward(object message)
        {
            if (message is Maybe<ChainCall> mayChain)
            {
                if (mayChain.IsNothing())
                {
                    _errorMessage = May("ChainError");
                    Finish(false);
                    return false;
                }

                message = mayChain.Value;
            }

            Self.Forward(message);
            return true;
        }

        private Maybe<bool> ProgressConditions(StepRev<TStep, TContext> rev, bool finish = false, Maybe<ChainCall> baseCall = default)
        {
            var mayStd = May(
                (
                    from con in rev.Conditions
                    let stateId = con.Select(rev.Step, RunContext)
                    where stateId.Name != StepId.None.Name
                    select stateId
                ).ToArray()
            );

            return Any(
                () =>
                    from ids in mayStd
                    where ids.Length != 0
                    select SelfForeward(new ChainCall(ids).WithBase(baseCall)),
                () =>
                    from condition in rev.GenericCondition
                    from cid in May(condition.Select(rev.Step, RunContext))
                    where cid != StepId.None
                    select SelfForeward(new ChainCall(cid).WithBase(baseCall)),
                () =>
                    from tryFinish in May(finish) 
                    where tryFinish
                    select Finish(false)
            ).Or(May(true));
        }

        protected void OnFinish(Action<WorkflowResult<TContext>> con)
            => _onFinish = Or(from del in _onFinish
                              select del.Combine(con), con);

        private bool Finish(bool isok, Maybe<StepRev<TStep, TContext>> mayRev = default)
        {
            _starterSender = Maybe<IActorRef>.Nothing;
            _runState = May(RunState.Stoped);

            Do(from rev in mayRev
               select Use(() => rev.Step.OnExecuteFinish(RunContext)));

            Self.Forward(new WorkflowResult<TContext>(isok, _errorMessage, RunContext));
            RunContext = Maybe<TContext>.Nothing;
            _errorMessage = Maybe<string>.Nothing;

            return true;
        }

        public void Start(Maybe<TContext> context)
        {
            _starterSender = Or(MayNotNull(Sender), ActorRefs.Nobody);
            _runState = May(RunState.Running);
            RunContext = context;
            Self.Forward(new ChainCall(StepId.Start));
        }

        protected StepConfiguration<TStep, TContext> WhenStep(StepId id, TStep step)
        {
            var rev = new StepRev<TStep, TContext>(step);
            _steps[id] = rev;
            return new StepConfiguration<TStep, TContext>(rev);
        }

        private sealed record TimeoutMarker;
        private sealed record LoopElement(StepRev<TStep, TContext> Rev, ChainCall Call);
    
        private sealed class ChainCall
        {
            private Maybe<ChainCall> BaseCall { get; }

            private StepId[] StepIds { get; }

            private Maybe<int> Position { get; }

            private ChainCall(StepId[] stepIds, Maybe<int> position, Maybe<ChainCall> baseCall = default)
            {
                BaseCall = baseCall;
                StepIds = stepIds;
                Position = position;
            }

            public ChainCall(StepId id, Maybe<ChainCall> baseCall = default)
            {
                BaseCall = baseCall;
                StepIds = new[] { id };
                Position = May(0);
            }

            public ChainCall(StepId[] ids)
            {
                StepIds = ids;
                Position = May(0);
            }

            public Maybe<StepId> Id
                => Or(
                    from pos in Position
                    where pos >= StepIds.Length
                    select StepIds[pos],

                    from baseCall in BaseCall
                    select baseCall.Id);

            public Maybe<ChainCall> Next()
            {
                var newPos = from oldPos in Position
                             select oldPos + 1;

                return Any
                (
                    () => Collapse(
                        from baseCall in BaseCall
                        from pos in Position
                        where pos == StepIds.Length
                        select baseCall.Next()),

                    () => May(new ChainCall(StepIds, newPos))
                );
            }

            public Maybe<ChainCall> WithBase(Maybe<ChainCall> mayCall)
            {
                return Any(
                    () =>
                        from call in mayCall 
                        from next in call.Next()
                        select new ChainCall(next.StepIds, next.Position, May(this)),
                    () => May(this)
                );
            }
        }
    }
}