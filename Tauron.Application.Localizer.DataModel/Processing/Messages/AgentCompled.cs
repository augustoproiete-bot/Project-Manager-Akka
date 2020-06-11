using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class AgentCompled
    {
        public bool Failed { get; }

        public Exception? Cause { get; }

        public string OperationId { get; private set; }

        public AgentCompled(bool failed, Exception? cause, string operationId)
        {
            Failed = failed;
            Cause = cause;
            OperationId = operationId;
        }
    }
}