using System;
using Functional.Maybe;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record LoadedProjectFile(string OperationId, ProjectFile ProjectFile, Maybe<Exception> ErrorReason, bool Ok) : Operation(OperationId);
}