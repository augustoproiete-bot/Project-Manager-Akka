using JetBrains.Annotations;

namespace Tauron.Application.Akka.ServiceResolver.Messages
{
    [PublicAPI]
    public sealed class ServiceCallRejected
    {
        public const string Error = "Exception";
        public const string Suspended = "Suspended";

        public ServiceCallRejected(object originalMessage, string reason, object? detail = null)
        {
            OriginalMessage = originalMessage;
            Detail = detail;
            Reason = reason;
        }

        public object OriginalMessage { get; }

        public object? Detail { get; }

        public string Reason { get; }
    }
}