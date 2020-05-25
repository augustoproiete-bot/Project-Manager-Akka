﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class TimerScheduler : SchedulerBase, IDateTimeOffsetNowTimeProvider, IDisposable
    {
        private static readonly Stopwatch Stopwatch = new Stopwatch();


        internal class Disposer : IDisposable
        {
            private IDisposable? _disposable;

            public void Set(IDisposable disposable)
                => _disposable = disposable;

            public void Dispose() => _disposable?.Dispose();
        }
        private class Registration : IDisposable
        {
            private readonly Action _runner;
            private readonly ICancelable? _cancelable;
            private readonly string _id;
            private readonly Action<string> _remove;
            private readonly Timer _timer;

            public Registration(Action runner, TimeSpan delay, TimeSpan interval, ICancelable? cancelable, string id, Action<string> remove)
            {
                _runner = runner;
                _cancelable = cancelable;
                _id = id;
                _remove = remove;

                _timer = new Timer(Run, null, delay, interval);    
            }

            private void Run(object? state)
            {
                if (_cancelable?.IsCancellationRequested == true)
                {
                    _timer.Dispose();
                    _remove(_id);
                }

                _runner();
            }

            public void Dispose() => _timer.Dispose();
        }

        private readonly ConcurrentDictionary<string, Registration> _registrations = new ConcurrentDictionary<string, Registration>();
        private readonly BlockingCollection<(Action, IDisposable)> _toRun = new BlockingCollection<(Action, IDisposable)>();
        private int _isDiposed;

        public TimerScheduler(Config scheduler, ILoggingAdapter log) 
            : base(scheduler, log)
        {
            new Thread(() =>
                       {
                           foreach (var action in _toRun.GetConsumingEnumerable())
                           {
                               try
                               {
                                   action.Item1();
                               }
                               catch (Exception e)
                               {
                                   Log.Error(e, "Error On Shedule Task");
                                   action.Item2.Dispose();
                               }
                           }

                           _toRun.Dispose();

                       }){ IsBackground = true }.Start();
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
            if(_isDiposed == 1) return;

            var id = Guid.NewGuid().ToString();
            if(_registrations.ContainsKey(id))
                throw new InvalidOperationException("Guid Duplicate key in Scheduler");
            var dispoise = new Disposer();

            var registration = new Registration(() =>
                                                {
                                                    try
                                                    {
                                                        if (_toRun.IsAddingCompleted) return;
                                                        _toRun.Add((runner, dispoise));
                                                    }
                                                    catch (ObjectDisposedException)
                                                    { }
                                                }, delay, interval, cancelable, id, key => _registrations.TryRemove(key, out _));
            dispoise.Set(registration);

            _registrations[id] = registration;
        }

        protected override DateTimeOffset TimeNow => DateTimeOffset.Now;
        public override TimeSpan MonotonicClock => Stopwatch.Elapsed;
        public override TimeSpan HighResMonotonicClock => Stopwatch.Elapsed;

        public void Dispose()
        {
            Interlocked.Exchange(ref _isDiposed, 1);

            foreach (var registration in _registrations) 
                registration.Value.Dispose();

            _registrations.Clear();
            _toRun.CompleteAdding();
        }
    }
}