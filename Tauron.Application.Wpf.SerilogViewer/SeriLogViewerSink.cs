using System;
using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Tauron.Application.Wpf.SerilogViewer
{
    public sealed class SeriLogViewerSink : ILogEventSink
    {
        public SeriLogViewerSink()
        {
            CurrentSink = this;
        }

        internal static SeriLogViewerSink? CurrentSink { get; private set; }

        public LimitedList<LogEvent> Logs { get; } = new LimitedList<LogEvent>(150);

        [DebuggerHidden]
        public void Emit(LogEvent logEvent)
        {
            Logs.Add(logEvent);
            LogReceived?.Invoke(logEvent);
        }

        public event Action<LogEvent>? LogReceived;
    }
}