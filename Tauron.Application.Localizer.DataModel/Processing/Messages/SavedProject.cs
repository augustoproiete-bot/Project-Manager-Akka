using System;
using Functional.Maybe;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record SavedProject(bool Ok, Maybe<Exception> Exception, string OperationId) : Operation(OperationId);
}