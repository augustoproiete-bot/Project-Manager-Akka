using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class Reporter
    {
        public static Reporter CreateReporter(IActorRefFactory factory, string name = "Reporter") => new Reporter(factory.ActorOf(Props.Create(() => new ReporterActor()), name));

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<OperationResult> onCompled, TimeSpan timeout,  string name = "LogListner") 
            => factory.ActorOf(Props.Create(() => new Listner(listner, onCompled, timeout)), name);

        public static IActorRef CreateListner(IActorRefFactory factory, Action<string> listner, Action<OperationResult> onCompled, string name = "LogListner")
            => factory.ActorOf(Props.Create(() => new Listner(listner, onCompled, Timeout.InfiniteTimeSpan)), name);

        private readonly IActorRef _reporter;

        public Reporter(IActorRef reporter) => _reporter = reporter;

        public void Listen(IActorRef actor) => _reporter.Tell(new ListeningActor(actor));

        public void Send(string message) => _reporter.Tell(new TransferedMessage(message));

        public void Compled(OperationResult result) => _reporter.Tell(result);

        private sealed class Listner : ReceiveActor, IWithTimers
        {
            public Listner(Action<string> listner, Action<OperationResult> onCompled, TimeSpan timeSpan)
            {
                Receive(onCompled);
                Receive<TransferedMessage>(m => listner(m.Message));
            }

            public ITimerScheduler Timers { get; set; }
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