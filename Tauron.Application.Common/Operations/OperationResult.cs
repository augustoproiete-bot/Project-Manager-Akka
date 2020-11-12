using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tauron.Operations
{
    public sealed record Error(string? Info, string Code);

    [PublicAPI]
    public interface IOperationResult
    {
        bool Ok { get; }
        Error[]? Errors { get; }
        object? Outcome { get; }
        string? Error { get; }
    }

    [PublicAPI]
    public sealed record OperationResult(bool Ok, Error[]? Errors, object? Outcome): IOperationResult
    {
        public static IOperationResult Success(object? result = null) => new OperationResult(true, null, result);

        public static IOperationResult Failure(Error error, object? outcome = null) => new OperationResult(false, new []{ error }, outcome);

        public static IOperationResult Failure(IEnumerable<Error> errors, object? outcome = null) => new OperationResult(false, errors.ToArray(), outcome);

        public static IOperationResult Failure(params Error[] errors) => new OperationResult(false, errors, null);

        public static IOperationResult Failure(Exception error)
        {
            var unwarped = error.Unwrap() ?? error;
            return new OperationResult(false, new[] {new Error(unwarped.Message, unwarped.HResult.ToString())}, null);
        }

        [JsonIgnore]
        public string? Error => Errors == null ? null : string.Join(", ", Errors.Select(e => e.Info ?? e.Code));
    }
}