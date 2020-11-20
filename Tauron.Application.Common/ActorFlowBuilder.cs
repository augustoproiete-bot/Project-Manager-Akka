using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using static Tauron.Prelude;

namespace Tauron
{
    public delegate void EnterFlow<in TStart>(TStart msg);

    public sealed class ExternalActorRecieveBuilder<TNext, TStart, TTarget> : ReceiveBuilderBase<TNext, TStart>
    {
        public ExternalActorRecieveBuilder([NotNull] ActorFlowBuilder<TStart> flow, Func<IActorContext, IActorRef> target, bool forward) 
            : base(flow)
        {
            if(forward)
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target(context).Forward(msg)));
            else
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target(context).Tell(msg, context.Self)));
        }

        public ExternalActorRecieveBuilder([NotNull] ActorFlowBuilder<TStart> flow, Func<IActorRef> target, bool forward)
            : base(flow)
        {
            if(forward)
                flow.Register(ad => ad.Receive<TTarget>((msg, _) => target().Forward(msg)));
            else
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target().Tell(msg, context.Self)));
        }
    }

    [PublicAPI]
    public class RunSelector<TRecieve, TStart>
    {
        public RunSelector(ActorFlowBuilder<TStart>? flow) 
            => Flow = flow!;

        public ActorFlowBuilder<TStart> Flow { get; protected set; }

        public FuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<Maybe<TRecieve>, Maybe<TNext>> transformer)
            => new(Flow, transformer);

        public AsyncFuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<Maybe<TRecieve>, Task<Maybe<TNext>>> transformer)
            => new(Flow, transformer);

        public FuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<Maybe<TNext>> transformer)
            => new(Flow, _ => transformer());

        public AsyncFuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<Task<Maybe<TNext>>> transformer)
            => new(Flow, _ => transformer());

        public ActionFinisher<TRecieve, TStart> Action(Action<Maybe<TRecieve>> act) 
            => new(Flow, act);

        public ActionFinisher<TRecieve, TStart> Action(Func<Maybe<TRecieve>, Task> act) 
            => new(Flow, act);

        public ActionFinisher<TRecieve, TStart> Action(Action act)
            => new(Flow, _ => act());

        public ActionFinisher<TRecieve, TStart> Action(Func<Task> act)
            => new(Flow, _ => act());

        public ActionFinisher<TRecieve, TStart> Action(Func<Maybe<TRecieve>, Maybe<Unit>> act)
            => new(Flow, r => act(r));

        public ActionFinisher<TRecieve, TStart> Action(Func<Maybe<TRecieve>, Task<Maybe<Unit>>> act)
            => new(Flow, act);

        public ActionFinisher<TRecieve, TStart> Action(Func<Maybe<Unit>> act)
            => new(Flow, _ => act());

        public ActionFinisher<TRecieve, TStart> Action(Func<Task<Maybe<Unit>>> act)
            => new(Flow, _ => act());

        public ExternalActorRecieveBuilder<TRespond, TStart, TRecieve> External<TRespond>(Func<IActorRef> target, bool forward = false)
            => new(Flow, target, forward);

        public ExternalActorRecieveBuilder<TRespond, TStart, TRecieve> External<TRespond>(Func<IActorContext, IActorRef> target, bool forward = false)
            => new(Flow, target, forward);

        public ActionFinisher<TRecieve, TStart> External(Func<IActorRef> target, bool forward = false)
        {
            return forward 
                ? new ActionFinisher<TRecieve, TStart>(Flow, recieve => Do(recieve, message => target().Forward(message))) 
                : new ActionFinisher<TRecieve, TStart>(Flow, recieve => Do(recieve, message => target().Tell(message)));
        }

        public ActionFinisher<TRecieve, TStart> External(Func<IActorContext, IActorRef> target, bool forward = false)
        {
            return forward 
                ? new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => Do(recieve, message => target(context).Forward(message))) 
                : new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => Do(recieve, message => target(context).Tell(message)));
        }

        public ActionFinisher<TRecieve, TStart> External(Func<Maybe<IActorRef>> target, bool forward = false) => External(() => target().Value, forward);

        public ActionFinisher<TRecieve, TStart> External(Func<IActorContext, Maybe<IActorRef>> target, bool forward = false) => External(c => target(c).Value, forward);

        public ActionFinisher<TRecieve, TStart> External<TTransform>(Func<IActorRef> target, Func<Maybe<TRecieve>, Maybe<TTransform>> convert, bool forward = false)
        {
            return forward
                ? new ActionFinisher<TRecieve, TStart>(Flow, recieve => Do(convert(recieve), msg => Forward(target(), msg!)))
                : new ActionFinisher<TRecieve, TStart>(Flow, recieve => Do(convert(recieve), msg => Tell(target(), msg!)));
        }

        public ActionFinisher<TRecieve, TStart> External<TTransform>(Func<IActorContext, IActorRef> target, Func<Maybe<TRecieve>, Maybe<TTransform>> convert, bool forward = false)
        {
            return forward
                ? new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => Do(convert(recieve), msg => Forward(target(context), msg!)))
                : new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => Do(convert(recieve), msg => Tell(target(context), msg!)));
        }
    }

    [PublicAPI]
    public abstract class AbastractTargetSelector<TRespond, TStart>
    {
        protected AbastractTargetSelector(ActorFlowBuilder<TStart> flow) 
            => Flow = flow;

        public ActorFlowBuilder<TStart> Flow { get; }

        public TRespond ToSelf() => ToRef(new RefFunc(RefFuncMode.Self).Send);

        public TRespond ToParent() => ToRef(new RefFunc(RefFuncMode.Parent).Send);

        public TRespond ToSender() => ToRef(new RefFunc(RefFuncMode.Sender).Send);

        public TRespond ToRef(IActorRef actorRef) => ToRef(_ => actorRef);

        public abstract TRespond ToRef(Func<IActorContext, IActorRef> actorRef);

        private enum RefFuncMode
        {
            Self,
            Sender,
            Parent
        }

        private class RefFunc
        {
            private readonly RefFuncMode _mode;

            public RefFunc(RefFuncMode mode) => _mode = mode;

            public IActorRef Send(IActorContext context)
            {
                return _mode switch
                {
                    RefFuncMode.Self => context.Self,
                    RefFuncMode.Sender => context.Sender,
                    RefFuncMode.Parent => context.Parent,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    [PublicAPI]
    public abstract class AbastractTargetSelectorForward<TRespond, TStart, TSelector> : AbastractTargetSelector<TRespond, TStart>
        where TSelector : AbastractTargetSelectorForward<TRespond, TStart, TSelector>
    {
        protected AbastractTargetSelectorForward(ActorFlowBuilder<TStart> flow)
            : base(flow){}

        protected bool ShouldForward { get; private set; }

        public virtual TSelector Forward
        {
            get
            {
                ShouldForward = true;
                return (TSelector)this;
            }
        }
    }

    [PublicAPI]
    public sealed class FuncTargetSelector<TRecieve, TNext, TStart> 
        : AbastractTargetSelectorForward<ReceiveBuilder<TRecieve, TNext, TStart>, TStart, FuncTargetSelector<TRecieve, TNext, TStart>>
    {
        private readonly Func<Maybe<TRecieve>, Maybe<TNext>> _transformer;

        public FuncTargetSelector(ActorFlowBuilder<TStart> flow, Func<Maybe<TRecieve>, Maybe<TNext>> transformer)
            : base(flow) =>
            _transformer = transformer;

        public override ReceiveBuilder<TRecieve, TNext, TStart> ToRef(Func<IActorContext, IActorRef> actorRef) 
            => new(Flow, actorRef, _transformer, ShouldForward);

        public ReceiveBuilder<TRecieve, TNext, TStart> ToRefFromMsg(Func<Maybe<TRecieve>, Maybe<IActorRef>> actorRef)
            => new(Flow, actorRef, _transformer, ShouldForward);
    }

    [PublicAPI]
    public sealed class AsyncFuncTargetSelector<TRecieve, TNext, TStart> 
        : AbastractTargetSelectorForward<AyncReceiveBuilder<TRecieve, TNext, TStart>, TStart, AsyncFuncTargetSelector<TRecieve, TNext, TStart>>
    {
        private readonly Func<Maybe<TRecieve>, Task<Maybe<TNext>>> _transformer;

        public AsyncFuncTargetSelector(ActorFlowBuilder<TStart> flow, Func<Maybe<TRecieve>, Task<Maybe<TNext>>> transformer)
            : base(flow)
        {
            _transformer = transformer;
        }

        public override AyncReceiveBuilder<TRecieve, TNext, TStart> ToRef(Func<IActorContext, IActorRef> actorRef)
        {
            return new(Flow, actorRef, _transformer, ShouldForward);
        }
    }

    [PublicAPI]
    public sealed class ActionFinisher<TRecieve, TStart>
    {
        private readonly ActorFlowBuilder<TStart> _flow;

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Action<Maybe<TRecieve>> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Receive<TRecieve>(new ActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Func<Maybe<TRecieve>, Task> runner)
        {
            _flow = flow;
            _flow.Register(a => a.ReceiveAsync<TRecieve>(new AsyncActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Action<IActorContext, Maybe<TRecieve>> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Receive<TRecieve>(new ActionRespondContext(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Func<IActorContext, Maybe<TRecieve>, Task> runner)
        {
            _flow = flow;
            _flow.Register(a => a.ReceiveAsync<TRecieve>(new AsyncActionRespondContext(runner).Run));
        }

        public EnterFlow<TStart> AndBuild() => _flow.Build();

        public ActionFinisher<TRecieve, TStart> Then<TResult>(Action<RunSelector<TResult, TStart>> builder)
        {
            builder(new RunSelector<TResult, TStart>(_flow));
            return this;
        }

        private sealed class ActionRespond
        {
            private readonly Action<Maybe<TRecieve>> _runner;

            public ActionRespond(Action<Maybe<TRecieve>> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context) => _runner(May(recieve));
        }

        private sealed class AsyncActionRespond
        {
            private readonly Func<Maybe<TRecieve>, Task> _runner;

            public AsyncActionRespond(Func<Maybe<TRecieve>, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(May(recieve));
        }

        private sealed class ActionRespondContext
        {
            private readonly Action<IActorContext, Maybe<TRecieve>> _runner;

            public ActionRespondContext(Action<IActorContext, Maybe<TRecieve>> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context) => _runner(context, May(recieve));
        }

        private sealed class AsyncActionRespondContext
        {
            private readonly Func<IActorContext, Maybe<TRecieve>, Task> _runner;

            public AsyncActionRespondContext(Func<IActorContext, Maybe<TRecieve>, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(context, May(recieve));
        }

        //private sealed class ReceiveHelper
        //{
        //    private readonly EnterFlow<TStart> _invoker;

        //    public ReceiveHelper(EnterFlow<TStart> invoker)
        //    {
        //        _invoker = invoker;
        //    }

        //    public void Send(TStart msg, IActorContext context)
        //    {
        //        _invoker(msg);
        //    }
        //}
    }

    [PublicAPI]
    public abstract class ReceiveBuilderBase<TNext, TStart>
    {
        protected ReceiveBuilderBase(ActorFlowBuilder<TStart> flow)
        {
            Flow = flow;
        }

        public ActorFlowBuilder<TStart> Flow { get; }

        public void Then(Action<RunSelector<TNext, TStart>> builder) => builder(new RunSelector<TNext, TStart>(Flow));
        public void Then<TMsg>(Action<RunSelector<TMsg, TStart>> builder) => builder(new RunSelector<TMsg, TStart>(Flow));
    }

    [PublicAPI]
    public class ReceiveBuilder<TReceive, TNext, TStart> : ReceiveBuilderBase<TNext, TStart>
    {
        public ReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<IActorContext, IActorRef> target, Func<Maybe<TReceive>, Maybe<TNext>> transformer, bool shouldForward)
            : base(flow)
        {
            flow.Register(a => a.Receive<TReceive>(new Receive(target, transformer, shouldForward).Run));
        }

        public ReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<Maybe<TReceive>, Maybe<IActorRef>> target, Func<Maybe<TReceive>, Maybe<TNext>> transformer, bool shouldForward)
            : base(flow)
        {
            flow.Register(a => a.Receive<TReceive>(new ReceiveFromMessage(target, transformer, shouldForward).Run));
        }

        private sealed class Receive
        {
            private readonly Func<IActorContext, IActorRef> _target;
            private readonly Func<Maybe<TReceive>, Maybe<TNext>> _transformer;
            private readonly bool _shouldForward;

            public Receive(Func<IActorContext, IActorRef> target, Func<Maybe<TReceive>, Maybe<TNext>> transformer, bool shouldForward)
            {
                _target = target;
                _transformer = transformer;
                _shouldForward = shouldForward;
            }


            public void Run(TReceive rec, IActorContext context)
            {
                var result = _transformer(rec.ToMaybe());

                result.Do(res =>
                {
                    if (res == null) return;

                    if (_shouldForward)
                        _target(context).Forward(res);
                    else
                        _target(context).Tell(res, context.Self);
                });
            }
        }

        private sealed class ReceiveFromMessage
        {
            private readonly Func<Maybe<TReceive>, Maybe<IActorRef>> _target;
            private readonly Func<Maybe<TReceive>, Maybe<TNext>> _transformer;
            private readonly bool _shouldForward;

            public ReceiveFromMessage(Func<Maybe<TReceive>, Maybe<IActorRef>> target, Func<Maybe<TReceive>, Maybe<TNext>> transformer, bool shouldForward)
            {
                _target = target;
                _transformer = transformer;
                _shouldForward = shouldForward;
            }


            public void Run(TReceive rec, IActorContext context)
            {
                var mayRec = May(rec);
                var result = _transformer(mayRec);

                result.Do(res =>
                {
                    if(res == null) return;

                    if (_shouldForward)
                        Forward(_target(mayRec), res);
                    else
                        Tell(_target(mayRec), res, context.Self);
                });
            }
        }

        //public void AndReceive()
        //{
        //    Flow.BuildReceive();
        //}
    }

    [PublicAPI]
    public class AyncReceiveBuilder<TReceive, TNext, TStart> : ReceiveBuilderBase<TNext, TStart>
    {
        public AyncReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<IActorContext, IActorRef> target, Func<Maybe<TReceive>, Task<Maybe<TNext>>> transformer, bool shouldForward)
            : base(flow)
        {
            flow.Register(a => a.ReceiveAsync<TReceive>(new Receive(target, transformer, shouldForward).Run));
        }

        private sealed class Receive
        {
            private readonly Func<IActorContext, IActorRef> _target;
            private readonly Func<Maybe<TReceive>, Task<Maybe<TNext>>> _transformer;
            private readonly bool _shouldForward;

            public Receive(Func<IActorContext, IActorRef> target, Func<Maybe<TReceive>, Task<Maybe<TNext>>> transformer, bool shouldForward)
            {
                _target = target;
                _transformer = transformer;
                _shouldForward = shouldForward;
            }

            public async Task Run(TReceive rec, IActorContext context)
            {
                var result = await _transformer(May(rec));

                result.Do(res =>
                {
                    if (_shouldForward)
                        _target(context).Forward(res);
                    else
                        _target(context).Tell(res, context.Self);
                });
            }
        }
    }

    public class ActorFlowBuilderTarget<TStart> 
        : AbastractTargetSelectorForward<ActorFlowBuilder<TStart>, TStart, ActorFlowBuilderTarget<TStart>>
    {
        private readonly ActorFlowBuilder<TStart> _flow;
        private readonly Action<bool, IActorRef> _sendTo;

        public ActorFlowBuilderTarget([NotNull] ActorFlowBuilder<TStart> flow, Action<bool, IActorRef> sendTo)
            : base(flow)
        {
            _flow = flow;
            _sendTo = sendTo;
        }

        public override ActorFlowBuilder<TStart> ToRef(Func<IActorContext, IActorRef> actorRef)
        {
            _sendTo(ShouldForward, actorRef(ExposedReceiveActor.ExposedContext));
            return _flow;
        }
    }

    [PublicAPI]
    public sealed class ActorFlowBuilder<TStart> : RunSelector<TStart, TStart>
    {
        private readonly List<Func<EnterFlow<TStart>>> _delgators = new();

        private int _recieves;

        public ActorFlowBuilder(IExposedReceiveActor actor)    
            : base(null)
        {
            Flow = this;
            Actor = actor;
        }

        public IExposedReceiveActor Actor { get; }

        //public RunSelector<TStart, TStart> From => new RunSelector<TStart, TStart>(this);

        public ActorFlowBuilderTarget<TStart> Send
            => new(this, (f, reff) => _delgators.Add(() => new Delegator(f, reff).Tell));
        
        public void Register(Action<IActorDsl> actorRegister)
        {
            _recieves++;
            actorRegister(Actor.Exposed);
        }

        internal EnterFlow<TStart> Build()
        {
            EnterFlow<TStart>? func = null;
            if (_recieves > 0)
                func = new EntryPoint(ExposedReceiveActor.ExposedContext.Self).Tell;

            return _delgators.Aggregate(func, (current, delgator) => current.Combine(delgator())) ?? (_ => { });
        }

        public TReturn OnTrigger<TReturn>(Func<EnterFlow<TStart>, TReturn> config) => config(Build());

        public void OnTrigger(Action<EnterFlow<TStart>> config) => config(Build());

        private sealed class EntryPoint
        {
            private readonly IActorRef _actor;

            public EntryPoint(IActorRef actor)
            {
                _actor = actor;
            }

            public void Tell(TStart start)
            {
                if (start == null) return;
                _actor.Tell(start, _actor);
            }
        }

        private sealed class Delegator
        {
            private readonly bool _forward;
            private readonly IActorRef _delegator;

            public Delegator(bool forward, IActorRef delegator)
            {
                _forward = forward;
                _delegator = delegator;
            }

            public void Tell(TStart starter)
            {
                if (starter == null) return;
                if(_forward)
                    _delegator.Forward(starter);
                else
                    _delegator.Tell(starter);
            }
        }
    }

    [PublicAPI]
    public static class ActorFlowExtensions
    {
    }
}