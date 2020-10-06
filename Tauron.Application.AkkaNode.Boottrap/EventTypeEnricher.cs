using System.Diagnostics;
using System.Text;
using Akka.Util;
using Serilog.Core;
using Serilog.Events;

namespace Tauron.Application.AkkaNode.Bootstrap
{
    [DebuggerStepThrough]
    public sealed class EventTypeEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var bytes = Encoding.UTF8.GetBytes(logEvent.MessageTemplate.Text);
            var hash = MurmurHash.ByteHash(bytes);
            var eventType = propertyFactory.CreateProperty("EventType", hash.ToString("x8"));
            logEvent.AddPropertyIfAbsent(eventType);
        }
    }
}