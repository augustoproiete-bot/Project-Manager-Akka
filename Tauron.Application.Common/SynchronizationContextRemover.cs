using System;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron
{
    #pragma warning disable CA1822 // Member als statisch markieren
    [PublicAPI]
    public class SynchronizationContextRemover : INotifyCompletion
    {
        public static SynchronizationContextRemover Run => new ();

        public bool IsCompleted => SynchronizationContext.Current == null;

        public void OnCompleted(Action continuation)
        {
            SynchronizationContext? current = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                continuation();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(current);
            }
        }

        public SynchronizationContextRemover GetAwaiter()
        {
            return this;
        }


        public void GetResult()

        {
        }
    }
}