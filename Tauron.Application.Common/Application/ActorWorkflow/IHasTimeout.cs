using System;

namespace Tauron.Application.ActorWorkflow
{
    public interface IHasTimeout
    {
        TimeSpan? Timeout { get; }
    }
}