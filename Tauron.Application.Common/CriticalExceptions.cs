using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron
{
    [PublicAPI]
    public static class CriticalExceptions
    {
        public static bool IsCriticalApplicationException(this Exception? ex)
        {
            ex = Unwrap(ex);
            return ex is StackOverflowException || ex is OutOfMemoryException || ex is ThreadAbortException
                   || ex is SecurityException;
        }


        public static bool IsCriticalException(this Exception? ex)
        {
            ex = Unwrap(ex);
            return ex is NullReferenceException || ex is StackOverflowException || ex is OutOfMemoryException
                   || ex is ThreadAbortException || ex is SEHException || ex is SecurityException;
        }


        public static Exception? Unwrap(this Exception? ex)
        {
            if (ex is AggregateException ex2) return ex2.Flatten().InnerExceptions[0];
            while (ex?.InnerException != null && ex is TargetInvocationException) ex = ex.InnerException;

            return ex;
        }
    }
}