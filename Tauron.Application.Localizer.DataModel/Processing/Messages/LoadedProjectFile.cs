using System;
using Functional.Maybe;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record LoadedProjectFile(ProjectFile ProjectFile, Maybe<Exception> ErrorReason, bool Ok, string OperationId) : Operation(OperationId);
}