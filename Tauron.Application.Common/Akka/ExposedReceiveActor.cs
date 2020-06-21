using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    public interface IExposedReceiveActor
    {
        IActorDsl Exposed { get; }
        IUntypedActorContext ExposedContext { get; }
    }

    [PublicAPI]
    public class ExposedReceiveActor : ReceiveActor, IActorDsl, IExposedReceiveActor
    {
        private Action<Exception, IActorContext>? _onPostRestart;
        private Action<IActorContext>? _onPostStop;
        private Action<Exception, object, IActorContext>? _onPreRestart;
        private Action<IActorContext>? _onPreStart;
        private SupervisorStrategy? _strategy;

        public IActorDsl Exposed => this;

        public IUntypedActorContext ExposedContext => Context;

        protected internal ILoggingAdapter Log { get; } = Context.GetLogger();

        void IActorDsl.Receive<T>(Action<T, IActorContext> handler) => Receive<T>(m => handler(m, Context));

        void IActorDsl.Receive<T>(Predicate<T> shouldHandle, Action<T, IActorContext> handler) => Receive(shouldHandle, obj => handler(obj, Context));

        void IActorDsl.Receive<T>(Action<T, IActorContext> handler, Predicate<T> shouldHandle) => Receive(t => handler(t, Context), shouldHandle);

        void IActorDsl.ReceiveAny(Action<object, IActorContext> handler) => ReceiveAny(m => handler(m, Context));

        void IActorDsl.ReceiveAsync<T>(Func<T, IActorContext, Task> handler, Predicate<T> shouldHandle) => ReceiveAsync(m => handler(m, Context), shouldHandle);

        void IActorDsl.ReceiveAsync<T>(Predicate<T> shouldHandle, Func<T, IActorContext, Task> handler) => ReceiveAsync(shouldHandle, arg => handler(arg, Context));

        void IActorDsl.ReceiveAnyAsync(Func<object, IActorContext, Task> handler) => ReceiveAnyAsync(m => handler(m, Context));

        void IActorDsl.DefaultPreRestart(Exception reason, object message) => base.PreRestart(reason, message);

        void IActorDsl.DefaultPostRestart(Exception reason) => PostRestart(reason);

        void IActorDsl.DefaultPreStart() => base.PreStart();

        void IActorDsl.DefaultPostStop() => base.PostStop();

        void IActorDsl.Become(Action<object, IActorContext> handler) => Become(o => handler(o, Context));

        void IActorDsl.BecomeStacked(Action<object, IActorContext> handler) => BecomeStacked(o => handler(o, Context));

        void IActorDsl.UnbecomeStacked() => UnbecomeStacked();

        IActorRef IActorDsl.ActorOf(Action<IActorDsl> config, string name) => Context.ActorOf(config, name);

        Action<Exception, IActorContext>? IActorDsl.OnPostRestart
        {
            get => _onPostRestart;
            set => _onPostRestart = (Action<Exception, IActorContext>?) Delegate.Combine(_onPostRestart, value);
        }

        Action<Exception, object, IActorContext>? IActorDsl.OnPreRestart
        {
            get => _onPreRestart;
            set => _onPreRestart = (Action<Exception, object, IActorContext>?) Delegate.Combine(_onPostRestart, value);
        }

        Action<IActorContext>? IActorDsl.OnPostStop
        {
            get => _onPostStop;
            set => _onPostStop = (Action<IActorContext>?) Delegate.Combine(_onPostStop, value);
        }

        Action<IActorContext>? IActorDsl.OnPreStart
        {
            get => _onPreStart;
            set => _onPreStart = (Action<IActorContext>?) Delegate.Combine(_onPreStart, value);
        }

        SupervisorStrategy? IActorDsl.Strategy
        {
            get => _strategy;
            set => _strategy = value;
        }

        protected override void PostRestart(Exception reason)
        {
            _onPostRestart?.Invoke(reason, Context);
            base.PostRestart(reason);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _onPreRestart?.Invoke(reason, message, Context);
            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            _onPostStop?.Invoke(Context);
            base.PostStop();
        }

        protected override void PreStart()
        {
            _onPreStart?.Invoke(Context);
            base.PreStart();
        }

        protected override SupervisorStrategy SupervisorStrategy() => _strategy ?? base.SupervisorStrategy();

        protected static Action<TMsg> When<TMsg>(Func<TMsg, bool> test, Action<TMsg> action)
        {
            return m =>
                   {
                       if (test(m))
                           action(m);
                   };
        }
    }
}