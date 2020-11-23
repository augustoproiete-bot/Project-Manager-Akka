using System;
using Akka.Actor;
using Akka.Event;
using Functional.Maybe;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public sealed class FuncLog
    {
        private readonly ILoggingAdapter _loggingAdapter;

        public FuncLog(ILoggingAdapter loggingAdapter) => _loggingAdapter = loggingAdapter;

        public bool IsDebugEnabled => _loggingAdapter.IsDebugEnabled;

        public bool IsInfoEnabled => _loggingAdapter.IsInfoEnabled;

        public bool IsWarningEnabled => _loggingAdapter.IsWarningEnabled;

        public bool IsErrorEnabled               => _loggingAdapter.IsErrorEnabled;
        public bool IsEnabled(LogLevel logLevel) => _loggingAdapter.IsEnabled(logLevel);

        public Maybe<Unit> Debug(string format, params object[] args)
        {
            _loggingAdapter.Debug(format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Debug(Exception cause, string format, params object[] args)
        {
            _loggingAdapter.Debug(cause, format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Info(string format, params object[] args)
        {
            _loggingAdapter.Info(format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Info(Exception cause, string format, params object[] args)
        {
            _loggingAdapter.Info(cause, format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Warning(string format, params object[] args)
        {
            _loggingAdapter.Warning(format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Warning(Exception cause, string format, params object[] args)
        {
            _loggingAdapter.Warning(cause, format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Error(string format, params object[] args)
        {
            _loggingAdapter.Error(format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Error(Exception cause, string format, params object[] args)
        {
            _loggingAdapter.Error(cause, format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Log(LogLevel logLevel, string format, params object[] args)
        {
            _loggingAdapter.Log(logLevel, format, args);
            return Unit.MayInstance;
        }

        public Maybe<Unit> Log(LogLevel logLevel, Exception cause, string format, params object[] args)
        {
            _loggingAdapter.Log(logLevel, cause, format, args);
            return Unit.MayInstance;
        }
    }
    
    public sealed class FuncTimerSheduler
    {
        private readonly ITimerScheduler _timerScheduler;

        public FuncTimerSheduler(ITimerScheduler timerScheduler) 
            => _timerScheduler = timerScheduler;

        public Maybe<Unit> StartPeriodicTimer(object key, object msg, TimeSpan interval)
        {
            _timerScheduler.StartPeriodicTimer(key, msg, interval);
            return Unit.MayInstance;
        }

        public Maybe<Unit> StartPeriodicTimer(object key, object msg, TimeSpan initialDelay, TimeSpan interval)
        {
            _timerScheduler.StartPeriodicTimer(key, msg, initialDelay, interval);
            return Unit.MayInstance;
        }

        public Maybe<Unit> StartSingleTimer(object key, object msg, TimeSpan timeout)
        {
            _timerScheduler.StartSingleTimer(key, msg, timeout);
            return Unit.MayInstance;
        }

        public Maybe<bool> IsTimerActive(object key) 
            => _timerScheduler.IsTimerActive(key).ToMaybe();

        public Maybe<Unit> Cancel(object key)
        {
            _timerScheduler.Cancel(key);
            return Unit.MayInstance;
        }

        public Maybe<Unit> CancelAll()
        {
            _timerScheduler.CancelAll();
            return Unit.MayInstance;
        }
    }
}