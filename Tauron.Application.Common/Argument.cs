using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class Argument
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Check(Func<Exception?> toCheck)
        {
            var ex = toCheck();

            if (ex == null) return;
            throw ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static TType NotNull<TType>([System.Diagnostics.CodeAnalysis.NotNull]
            TType toCheck, string parameterName)
        {
            Check(() => toCheck == null ? new ArgumentNullException(parameterName) : null);
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
            return toCheck!;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TType NotNull<TType>(TType toCheck, string parameterName, string message)
        {
            Check(() => toCheck == null ? new ArgumentNullException(parameterName, message) : null);
            return toCheck;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string NotNull(string? toCheck, string parameterName)
        {
            Check(() => string.IsNullOrWhiteSpace(toCheck) ? new ArgumentNullException(parameterName) : null);
            return toCheck!;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Check(bool toCheck, Func<Exception> exceptionBuilder)
        {
            Check(() => toCheck ? exceptionBuilder() : null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [return: System.Diagnostics.CodeAnalysis.NotNull]
        public static TValue CheckResult<TValue>([System.Diagnostics.CodeAnalysis.NotNull]
            TValue value, string name)
        {
            Check(() => value == null ? new ArgumentNullException(name) : null);
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
            return value!;
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
        }
    }
}