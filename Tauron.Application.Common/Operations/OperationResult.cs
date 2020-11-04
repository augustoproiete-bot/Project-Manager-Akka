using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tauron.Operations
{
    public interface IOperationResult
    {
        bool Ok { get; }
        string[]? Errors { get; }
        object? Outcome { get; }
        string? Error { get; }
    }

    [PublicAPI]
    public sealed class OperationResult : IOperationResult
    {
        public static IOperationResult Success(object? result = null) => new OperationResult(true, null, result);

        public static IOperationResult Failure(string error, object? outcome = null) => new OperationResult(false, new []{ error }, outcome);

        public static IOperationResult Failure(IEnumerable<string> errors, object? outcome = null) => new OperationResult(false, errors.ToArray(), outcome);
        public static IOperationResult Failure(params string[] errors) => new OperationResult(false, errors, null);

        public static IOperationResult Failure(Exception? error) => new OperationResult(false, new []{ error.Unwrap()?.Message ?? "Unkowen" }, null);

        public bool Ok { get; private set; }

        public string[]? Errors { get; private set; }
        
        public object? Outcome { get; private set; }

        [JsonIgnore]
        public string? Error => Errors == null ? null : string.Join(", ", Errors);

        [JsonConstructor]
        internal OperationResult(bool ok, string[]? error, object? outcome)
        {
            Ok = ok;
            Errors = error;
            Outcome = outcome;
        }
    }
}