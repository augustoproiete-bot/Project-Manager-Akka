using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class Reporter
    {
        public const string TimeoutError = nameof(TimeoutError);

        public static Reporter CreateReporter(IActorRefFactory factory, string name = "Reporter") 
            => new Reporter(factory.ActorOf(Props.Create(() => new ReporterActor()).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name));

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<OperationResult> onCompled, TimeSpan timeout,  string name = "LogListner") 
            => factory.ActorOf(Props.Create(() => new Listner(listner, onCompled, timeout)).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<OperationResult> onCompled, string name = "LogListner")
            => factory.ActorOf(Props.Create(() => new Listner(listner, onCompled, Timeout.InfiniteTimeSpan)).WithSupervisorStrategy(SupervisorStrategy.StoppingStrategy), name);

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

        public void Compled(OperationResult result)
        {
            if (_compledCalled.GetAndSet(true))
                throw new InvalidOperationException("Reporter is Compled");
            _reporter.Tell(result);
        }

        private sealed class Listner : ReceiveActor, IWithTimers
        {
            public Listner(Action<string> listner, Action<OperationResult> onCompled, TimeSpan timeSpan)
            {
                Receive<OperationResult>(c =>
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

                Receive<OperationResult>(msg =>
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

        private sealed class TransferedMessage : InternalSerializableBase
        {
            public string Message { get; private set; }

            public TransferedMessage(BinaryReader reader)
                : base(reader)
            {
                Message = string.Empty;
            }

            public TransferedMessage(string message)
            {
                Message = message;
            }

            protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest) => Message = reader.ReadString();

            protected override void WriteInternal(ActorBinaryWriter writer) => writer.Write(Message);
        }
    }
}