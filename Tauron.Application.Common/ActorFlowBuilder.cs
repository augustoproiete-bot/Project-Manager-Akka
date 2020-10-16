using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using JetBrains.Annotations;
using Tauron.Akka;

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
                flow.Register(ad => ad.Receive<TTarget>((msg, context) => target().Forward(msg)));
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

        public FuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<TRecieve, TNext> transformer)
            => new FuncTargetSelector<TRecieve, TNext, TStart>(Flow, transformer);

        public AsyncFuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<TRecieve, Task<TNext>> transformer)
            => new AsyncFuncTargetSelector<TRecieve, TNext, TStart>(Flow, transformer);

        public FuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<TNext> transformer)
            => new FuncTargetSelector<TRecieve, TNext, TStart>(Flow, _ => transformer());

        public AsyncFuncTargetSelector<TRecieve, TNext, TStart> Func<TNext>(Func<Task<TNext>> transformer)
            => new AsyncFuncTargetSelector<TRecieve, TNext, TStart>(Flow, _ => transformer());

        public ActionFinisher<TRecieve, TStart> Action(Action<TRecieve> act) 
            => new ActionFinisher<TRecieve, TStart>(Flow, act);

        public ActionFinisher<TRecieve, TStart> Action(Func<TRecieve, Task> act) 
            => new ActionFinisher<TRecieve, TStart>(Flow, act);

        public ActionFinisher<TRecieve, TStart> Action(Action act)
            => new ActionFinisher<TRecieve, TStart>(Flow, _ => act());

        public ActionFinisher<TRecieve, TStart> Action(Func<Task> act)
            => new ActionFinisher<TRecieve, TStart>(Flow, _ => act());

        public ExternalActorRecieveBuilder<TRespond, TStart, TRecieve> External<TRespond>(Func<IActorRef> target, bool forward = false)
            => new ExternalActorRecieveBuilder<TRespond, TStart, TRecieve>(Flow, target, forward);

        public ExternalActorRecieveBuilder<TRespond, TStart, TRecieve> External<TRespond>(Func<IActorContext, IActorRef> target, bool forward = false)
            => new ExternalActorRecieveBuilder<TRespond, TStart, TRecieve>(Flow, target, forward);

        public ActionFinisher<TRecieve, TStart> External(Func<IActorRef> target, bool forward = false)
        {
            return forward 
                ? new ActionFinisher<TRecieve, TStart>(Flow, recieve => target().Forward(recieve)) 
                : new ActionFinisher<TRecieve, TStart>(Flow, recieve => target().Tell(recieve));
        }

        public ActionFinisher<TRecieve, TStart> External(Func<IActorContext, IActorRef> target, bool forward = false)
        {
            return forward 
                ? new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => target(context).Forward(recieve)) 
                : new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => target(context).Tell(recieve));
        }

        public ActionFinisher<TRecieve, TStart> External<TTransform>(Func<IActorRef> target, Func<TRecieve, TTransform> convert, bool forward = false)
        {
            return forward
                ? new ActionFinisher<TRecieve, TStart>(Flow, recieve => target().Forward(convert(recieve)))
                : new ActionFinisher<TRecieve, TStart>(Flow, recieve => target().Tell(convert(recieve)));
        }

        public ActionFinisher<TRecieve, TStart> External<TTransform>(Func<IActorContext, IActorRef> target, Func<TRecieve, TTransform> convert, bool forward = false)
        {
            return forward
                ? new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => target(context).Forward(convert(recieve)))
                : new ActionFinisher<TRecieve, TStart>(Flow, (context, recieve) => target(context).Tell(convert(recieve)));
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

        public TRespond ToRef(IActorRef actorRef) => ToRef(c => actorRef);

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
        private readonly Func<TRecieve, TNext> _transformer;

        public FuncTargetSelector(ActorFlowBuilder<TStart> flow, Func<TRecieve, TNext> transformer)
            : base(flow) =>
            _transformer = transformer;

        public override ReceiveBuilder<TRecieve, TNext, TStart> ToRef(Func<IActorContext, IActorRef> actorRef) 
            => new ReceiveBuilder<TRecieve, TNext, TStart>(Flow, actorRef, _transformer, ShouldForward);

        public ReceiveBuilder<TRecieve, TNext, TStart> ToRefFromMsg(Func<TRecieve, IActorRef> actorRef)
            => new ReceiveBuilder<TRecieve, TNext, TStart>(Flow, actorRef, _transformer, ShouldForward);
    }

    [PublicAPI]
    public sealed class AsyncFuncTargetSelector<TRecieve, TNext, TStart> 
        : AbastractTargetSelectorForward<AyncReceiveBuilder<TRecieve, TNext, TStart>, TStart, AsyncFuncTargetSelector<TRecieve, TNext, TStart>>
    {
        private readonly Func<TRecieve, Task<TNext>> _transformer;

        public AsyncFuncTargetSelector(ActorFlowBuilder<TStart> flow, Func<TRecieve, Task<TNext>> transformer)
            : base(flow)
        {
            _transformer = transformer;
        }

        public override AyncReceiveBuilder<TRecieve, TNext, TStart> ToRef(Func<IActorContext, IActorRef> actorRef)
        {
            return new AyncReceiveBuilder<TRecieve, TNext, TStart>(Flow, actorRef, _transformer, ShouldForward);
        }
    }

    [PublicAPI]
    public sealed class ActionFinisher<TRecieve, TStart>
    {
        private readonly ActorFlowBuilder<TStart> _flow;

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Action<TRecieve> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Receive<TRecieve>(new ActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Func<TRecieve, Task> runner)
        {
            _flow = flow;
            _flow.Register(a => a.ReceiveAsync<TRecieve>(new AsyncActionRespond(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Action<IActorContext, TRecieve> runner)
        {
            _flow = flow;
            _flow.Register(a => a.Receive<TRecieve>(new ActionRespondContext(runner).Run));
        }

        public ActionFinisher(ActorFlowBuilder<TStart> flow, Func<IActorContext, TRecieve, Task> runner)
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
            private readonly Action<TRecieve> _runner;

            public ActionRespond(Action<TRecieve> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context) => _runner(recieve);
        }

        private sealed class AsyncActionRespond
        {
            private readonly Func<TRecieve, Task> _runner;

            public AsyncActionRespond(Func<TRecieve, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(recieve);
        }

        private sealed class ActionRespondContext
        {
            private readonly Action<IActorContext, TRecieve> _runner;

            public ActionRespondContext(Action<IActorContext, TRecieve> runner) => _runner = runner;

            public void Run(TRecieve recieve, IActorContext context) => _runner(context, recieve);
        }

        private sealed class AsyncActionRespondContext
        {
            private readonly Func<IActorContext, TRecieve, Task> _runner;

            public AsyncActionRespondContext(Func<IActorContext, TRecieve, Task> runner) => _runner = runner;

            public async Task Run(TRecieve recieve, IActorContext context) => await _runner(context, recieve);
        }

        private sealed class ReceiveHelper
        {
            private readonly EnterFlow<TStart> _invoker;

            public ReceiveHelper(EnterFlow<TStart> invoker)
            {
                _invoker = invoker;
            }

            public void Send(TStart msg, IActorContext context)
            {
                _invoker(msg);
            }
        }
    }

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
        public ReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<IActorContext, IActorRef> target, Func<TReceive, TNext> transformer, bool shouldForward)
            : base(flow)
        {
            flow.Register(a => a.Receive<TReceive>(new Receive(target, transformer, shouldForward).Run));
        }

        public ReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<TReceive, IActorRef> target, Func<TReceive, TNext> transformer, bool shouldForward)
            : base(flow)
        {
            flow.Register(a => a.Receive<TReceive>(new ReceiveFromMessage(target, transformer, shouldForward).Run));
        }

        private sealed class Receive
        {
            private readonly Func<IActorContext, IActorRef> _target;
            private readonly Func<TReceive, TNext> _transformer;
            private readonly bool _shouldForward;

            public Receive(Func<IActorContext, IActorRef> target, Func<TReceive, TNext> transformer, bool shouldForward)
            {
                _target = target;
                _transformer = transformer;
                _shouldForward = shouldForward;
            }


            public void Run(TReceive rec, IActorContext context)
            {
                var result = _transformer(rec);
                if (result == null) return;
                if(_shouldForward)
                    _target(context).Forward(result);
                else
                    _target(context).Tell(result, context.Self);
            }
        }

        private sealed class ReceiveFromMessage
        {
            private readonly Func<TReceive, IActorRef> _target;
            private readonly Func<TReceive, TNext> _transformer;
            private readonly bool _shouldForward;

            public ReceiveFromMessage(Func<TReceive, IActorRef> target, Func<TReceive, TNext> transformer, bool shouldForward)
            {
                _target = target;
                _transformer = transformer;
                _shouldForward = shouldForward;
            }


            public void Run(TReceive rec, IActorContext context)
            {
                var result = _transformer(rec);
                if (result == null) return;
                if (_shouldForward)
                    _target(rec).Forward(result);
                else
                    _target(rec).Tell(result, context.Self);
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
        public AyncReceiveBuilder(ActorFlowBuilder<TStart> flow, Func<IActorContext, IActorRef> target, Func<TReceive, Task<TNext>> transformer, bool shouldForward)
            : base(flow)
        {
            flow.Register(a => a.ReceiveAsync<TReceive>(new Receive(target, transformer, shouldForward).Run));
        }

        private sealed class Receive
        {
            private readonly Func<IActorContext, IActorRef> _target;
            private readonly Func<TReceive, Task<TNext>> _transformer;
            private readonly bool _shouldForward;

            public Receive(Func<IActorContext, IActorRef> target, Func<TReceive, Task<TNext>> transformer, bool shouldForward)
            {
                _target = target;
                _transformer = transformer;
                _shouldForward = shouldForward;
            }

            public async Task Run(TReceive rec, IActorContext context)
            {
                var result = await _transformer(rec);
                if (result == null) return;
                if(_shouldForward)
                    _target(context).Forward(result);
                else
                    _target(context).Tell(result, context.Self);
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
        private readonly List<Func<EnterFlow<TStart>>> _delgators = new List<Func<EnterFlow<TStart>>>();

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
            => new ActorFlowBuilderTarget<TStart>(this, (f, reff) => _delgators.Add(() => new Delegator(f, reff).Tell));
        
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

            return _delgators.Aggregate(func, (current, delgator) => current.Combine(delgator())) ?? (s => { });
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