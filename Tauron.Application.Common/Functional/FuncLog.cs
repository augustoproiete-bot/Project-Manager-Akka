using System;
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

        public bool IsDebugEnabled => _loggingAdapter.IsDebugEnabled;

        public bool IsInfoEnabled => _loggingAdapter.IsInfoEnabled;

        public bool IsWarningEnabled => _loggingAdapter.IsWarningEnabled;

        public bool IsErrorEnabled => _loggingAdapter.IsErrorEnabled;
    }
}