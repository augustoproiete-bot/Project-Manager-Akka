using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Prelude;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class TimerScheduler : SchedulerBase, IDateTimeOffsetNowTimeProvider, IDisposable
    {
        private static readonly Stopwatch Stopwatch = new();

        private readonly ConcurrentDictionary<string, Registration> _registrations = new();
        private readonly BlockingCollection<(Action, IDisposable)>  _toRun         = new();
        private          int                                        _isDiposed;

        public TimerScheduler(Config scheduler, ILoggingAdapter log)
            : base(scheduler, log)
        {
            StartLongTask(() =>
                          {
                              foreach (var action in _toRun.GetConsumingEnumerable())
                              {
                                  Try(() => action.Item1())
                                     .OnError(e =>
                                              {
                                                  Log.Error(e, "Error On Shedule Task");
                                                  action.Item2.Dispose();
                                              });
                              }

                              _toRun.Dispose();
                          });
        }

        protected override DateTimeOffset TimeNow               => DateTimeOffset.Now;
        public override    TimeSpan       MonotonicClock        => Stopwatch.Elapsed;
        public override    TimeSpan       HighResMonotonicClock => Stopwatch.Elapsed;

        public void Dispose()
        {
            Do(from diposed in May(Interlocked.Exchange(ref _isDiposed, 1))
               where diposed == 0
               select Action(() =>
                          {
                              foreach (var registration in _registrations)
                                  registration.Value.Dispose();

                              _registrations.Clear();
                              _toRun.CompleteAdding();
                          }));
        }

        protected override void InternalScheduleTellOnce(TimeSpan delay, ICanTell receiver, object message, IActorRef sender, ICancelable cancelable)
            => AddGeneric(() => receiver.Tell(message, sender), delay, Timeout.InfiniteTimeSpan, cancelable);

        protected override void InternalScheduleTellRepeatedly(TimeSpan initialDelay, TimeSpan interval, ICanTell receiver, object message, IActorRef sender, ICancelable cancelable)
            => AddGeneric(() => receiver.Tell(message, sender), initialDelay, interval, cancelable);

        protected override void InternalScheduleOnce(TimeSpan delay, Action action, ICancelable cancelable)
            => AddGeneric(action, delay, Timeout.InfiniteTimeSpan, cancelable);

        protected override void InternalScheduleRepeatedly(TimeSpan initialDelay, TimeSpan interval, Action action, ICancelable cancelable)
            => AddGeneric(action, initialDelay, interval, cancelable);

        private void AddGeneric(Action runner, TimeSpan delay, TimeSpan interval, ICancelable? cancelable)
        {
            Do(from disposed in May(_isDiposed)
               where disposed == 0
               select from id in May(Guid.NewGuid().ToString())
                      where !_registrations.ContainsKey(id)
                      select new Registration(d =>
                                              {
                                                  try
                                                  {
                                                      if (_toRun.IsAddingCompleted) return;
                                                      _toRun.Add((runner, d));
                                                  }
                                                  catch (ObjectDisposedException)
                                                  {
                                                  }
                                              }, delay, interval, cancelable, id, key => _registrations.TryRemove(key, out _)),
               r => _registrations[r.Id] = r);
        }

        private class Registration : IDisposable
        {
            private readonly ICancelable?        _cancelable;
            private readonly Action<string>      _remove;
            private readonly Action<IDisposable> _runner;
            private readonly Timer               _timer;

            public Registration(Action<IDisposable> runner, TimeSpan delay, TimeSpan interval, ICancelable? cancelable, string id, Action<string> remove)
            {
                _runner     = runner;
                _cancelable = cancelable;
                Id          = id;
                _remove     = remove;

                _timer = new Timer(Run, null, delay, interval);
            }

            public string Id { get; }

            public void Dispose() => _timer.Dispose();

            private void Run(object? state)
            {
                var dispose =
                    from cancel in MayNotNull(_cancelable)
                    where cancel.IsCancellationRequested
                    select Prelude.Action(() =>
                               {
                                   _timer.Dispose();
                                   _remove(Id);
                               });

                if (dispose.IsNothing())
                    _runner(this);
            }
        }
    }
}