using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class WorkDistributor<TInput, TFinishMessage>
    {
        private readonly IActorRef _actor;

        private WorkDistributor(IActorRef actor) => _actor = actor;

        public static WorkDistributor<TInput, TFinishMessage> Create(Props worker, string workerName, TimeSpan timeout, string? name = null)
            => Create(ExposedReceiveActor.ExposedContext, worker, workerName, timeout, name);

        public static WorkDistributor<TInput, TFinishMessage> Create(IActorRefFactory factory, Props worker, string workerName, TimeSpan timeout, string? name = null)
        {
            var actor = factory.ActorOf(() => new WorkDistributorActor(worker, workerName, timeout), name);
            return new WorkDistributor<TInput, TFinishMessage>(actor);
        }

        public void PushWork(TInput work)
            => _actor.Forward(work);

        private sealed class WorkDistributorActor : ExposedReceiveActor, IWithTimers
        {
            private int _id = 5;
            private readonly Props _workerProps;
            private readonly string _workerName;
            private readonly TimeSpan _timeout;

            private readonly Queue<(TInput, IActorRef)> _pendingWorkload = new Queue<(TInput, IActorRef)>();
            private readonly List<IActorRef> _worker = new List<IActorRef>();

            private readonly Queue<IActorRef> _ready = new Queue<IActorRef>();
            private readonly List<IActorRef> _running = new List<IActorRef>();

            public ITimerScheduler Timers { get; set; } = null!;

            public WorkDistributorActor(Props workerProps, string workerName, TimeSpan timeout)
            {
                _workerProps = workerProps;
                _workerName = workerName;
                _timeout = timeout;

                Receive<Terminated>(HandleTerminate);
                Receive<WorkerTimeout>(t =>
                {
                    if(_running.Contains(t.Worker))
                        Context.Stop(t.Worker);
                });

                foreach (var pos in Enumerable.Range(1, 5))
                {
                    var worker = Context.ActorOf(workerProps, $"{workerName}-{pos}");
                    Context.Watch(worker);
                    _worker.Add(worker);
                    _ready.Enqueue(worker);
                }

                Receive<TFinishMessage>(WorkFinish);
                Receive<TInput>(PushWork);
            }

            private void WorkFinish(TFinishMessage msg)
            {
                if (!_running.Contains(Context.Sender)) return;

                if (_pendingWorkload.TryDequeue(out var elemnt))
                {
                    var (input, sender) = elemnt;
                    RunWork(input, Sender, sender);
                }
                else
                {
                    _running.Remove(Context.Sender);
                    _ready.Enqueue(Context.Sender);
                }
            }

            private void HandleTerminate(Terminated terminated)
            {
                if (!_worker.Remove(terminated.ActorRef)) return;
                if (!_running.Remove(terminated.ActorRef))
                {
                    var temp = _ready.ToArray();
                    _ready.Clear();
                    foreach (var worker in temp)
                    {
                        if (worker.Equals(terminated.ActorRef)) continue;
                        _ready.Enqueue(worker);
                    }
                }

                _id++;
                var newWorker = Context.ActorOf(_workerProps, $"{_workerName}-{_id}");
                Context.Watch(newWorker);
                _ready.Enqueue(newWorker);
                _worker.Add(newWorker);
            }

            private void PushWork(TInput input)
            {
                if (_ready.TryDequeue(out var worker))
                {
                    RunWork(input, worker, Sender);
                    _running.Add(worker);
                }
                else
                    _pendingWorkload.Enqueue((input, Sender));
            }

            private void RunWork(TInput input, IActorRef worker, IActorRef sender)
            {
                worker.Tell(input, sender);
                Timers.StartSingleTimer(worker, new WorkerTimeout(worker), _timeout);
            }

            protected override SupervisorStrategy SupervisorStrategy() => global::Akka.Actor.SupervisorStrategy.StoppingStrategy;

            private sealed class WorkerTimeout
            {
                public IActorRef Worker { get; }

                public WorkerTimeout(IActorRef worker) => Worker = worker;
            }
        }
    }
}