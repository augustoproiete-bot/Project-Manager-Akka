using System;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class OperationResult
    {
        public static OperationResult Success(object? result = null) => new OperationResult(true, null, result);

        public static OperationResult Failure(string error) => new OperationResult(false, error, null);

        public static OperationResult Failure(Exception? error) => new OperationResult(false, error.Unwrap()?.Message ?? "Unkowen", null);

        public bool Ok { get; private set; }

        public string? Error { get; private set; }
        
        public object? Outcome { get; private set; }

        public OperationResult(bool ok, string? error, object? outcome)
        {
            Ok = ok;
            Error = error;
            Outcome = outcome;
        }
    }
}