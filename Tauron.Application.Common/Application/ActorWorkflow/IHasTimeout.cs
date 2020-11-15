using System;
using Functional.Maybe;

namespace Tauron.Application.ActorWorkflow
{
    public interface IHasTimeout
    {
        Maybe<TimeSpan> Timeout { get; }
    }
}