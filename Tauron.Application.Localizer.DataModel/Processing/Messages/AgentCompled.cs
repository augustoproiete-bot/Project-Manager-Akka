using System;
using Functional.Maybe;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record AgentCompled(bool Failed, Maybe<Exception> Cause, string OperationId);
}