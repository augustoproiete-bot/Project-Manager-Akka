using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class Reporter
    {
        public const string TimeoutError = nameof(TimeoutError);

        public static Reporter CreateReporter(IActorRefFactory factory, string? name = "Reporter") 
            => new Reporter(factory.ActorOf(Props.Create(() => new ReporterActor()).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name));

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<IOperationResult> onCompled, TimeSpan timeout,  string? name = "LogListner")
            => factory.ActorOf(Props.Create(() => new Listner(listner, onCompled, timeout)).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<IOperationResult> onCompled, string? name = "LogListner")
            => CreateListner(factory, listner, onCompled, Timeout.InfiniteTimeSpan, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter, Action<IOperationResult> onCompled, TimeSpan timeout, string? name = "LogListner")
            => CreateListner(factory, reporter.Send, onCompled, timeout, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TaskCompletionSource<IOperationResult> onCompled, TimeSpan timeout, string? name = "LogListner")
            => CreateListner(factory, listner, onCompled.SetResult, timeout, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TaskCompletionSource<IOperationResult> onCompled, string? name = "LogListner")
            => CreateListner(factory, listner, onCompled, Timeout.InfiniteTimeSpan, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter, TaskCompletionSource<IOperationResult> onCompled, TimeSpan timeout, string? name = "LogListner")
            => CreateListner(factory, reporter.Send, onCompled, timeout, name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout, string? name, Action<Task<IOperationResult>> onCompled)
        {
            var source = new TaskCompletionSource<IOperationResult>();
            var actor =  CreateListner(factory, listner, source, timeout, null);
            onCompled(source.Task);
            return actor;
        }

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, string name, Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, listner, Timeout.InfiniteTimeSpan, name, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter, TimeSpan timeout, string? name, Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, reporter.Send, timeout, name, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout, Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, listner, timeout, null, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, listner, Timeout.InfiniteTimeSpan, null, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Reporter reporter, TimeSpan timeout, Action<Task<IOperationResult>> onCompled)
            => CreateListner(factory, reporter.Send, timeout, null, onCompled);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout, string? name, out Task<IOperationResult> onCompled)
        {
            var source = new TaskCompletionSource<IOperationResult>();
            var actor = CreateListner(factory, listner, source, timeout, null);
            onCompled = source.Task;
            return actor;
        }

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, TimeSpan timeout, out Task<IOperationResult> onCompled)
            => CreateListner(factory, listner, timeout, null, out onCompled);

        private readonly IActorRef _reporter;
        private AtomicBoolean _compledCalled = new AtomicBoolean();

        public bool IsCompled => _compledCalled.Value;

        public Reporter(IActorRef reporter) => _reporter = reporter;

        public void Listen(IActorRef actor)
        {
            if(_compledCalled.Value)
                throw new InvalidOperationException("Reporter is Compled");
            _reporter.Tell(new ListeningActor(actor));
        }

        public void Send(string message)
        {
            if (_compledCalled.Value)
                throw new InvalidOperationException("Reporter is Compled");
            _reporter.Tell(new TransferedMessage(message));
        }

        public void Compled(IOperationResult result)
        {
            if (_compledCalled.GetAndSet(true))
                throw new InvalidOperationException("Reporter is Compled");
            _reporter.Tell(result);
        }

        private sealed class Listner : ReceiveActor, IWithTimers
        {
            public Listner(Action<string> listner, Action<IOperationResult> onCompled, TimeSpan timeSpan)
            {
                Receive<IOperationResult>(c =>
                {
                    Context.Stop(Self);
                    onCompled(c);
                });
                Receive<TransferedMessage>(m => listner(m.Message));

                if(timeSpan == Timeout.InfiniteTimeSpan)
                    return;

                Timers.StartSingleTimer(timeSpan, OperationResult.Failure(TimeoutError), timeSpan);
            }

            public ITimerScheduler Timers { get; set; } = null!;
        }

        private sealed class ReporterActor : ReceiveActor
        {
            private readonly List<IActorRef> _listner = new List<IActorRef>();

            public ReporterActor()
            {
                Receive<TransferedMessage>(msg =>
                {
                    foreach (var actorRef in _listner) actorRef.Forward(msg);
                });

                Receive<IOperationResult>(msg =>
                {
                    foreach (var actorRef in _listner) actorRef.Forward(msg);
                    Context.Stop(Self);
                });

                Receive<ListeningActor>(a =>
                {
                    Context.Watch(a.Actor);
                    _listner.Add(a.Actor);
                });

                Receive<Terminated>(t =>
                {
                    _listner.Remove(t.ActorRef);
                });
            }
        }

        private sealed class ListeningActor
        {
            public IActorRef Actor { get; }

            public ListeningActor(IActorRef actor) => Actor = actor;
        }

        private sealed class TransferedMessage
        {
            public string Message { get; private set; }

            public TransferedMessage(string message) => Message = message;
        }
    }
}