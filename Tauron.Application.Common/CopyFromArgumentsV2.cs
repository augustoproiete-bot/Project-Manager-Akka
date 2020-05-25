using System;
using System.Threading;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;

namespace Tauron
{
    /// <summary>
    /// The arguments for StreamHelper.CopyFrom(Stream, Stream, CopyFromArguments)
    /// </summary>
    [Record]
    [PublicAPI]
    public sealed partial class CopyFromArguments
    {
        public long TotalLength { get; } = -1;

        /// <summary>
        /// Gets or sets the size of the buffer used for copying bytes. Default is 4096.
        /// </summary>
        public int BufferSize { get; } = 4096;

        /// <summary>
        /// Gets or sets the callback for progress-report. Default is null.
        /// </summary>
        public ProgressChange? ProgressChangeCallback { get; }

        /// <summary>
        /// Gets or sets the event for aborting the operation. Default is null.
        /// </summary>
        public WaitHandle? StopEvent { get; }

        /// <summary>
        /// Gets or sets the time interval between to progress change callbacks. Default is 200 ms.
        /// </summary>
        public TimeSpan ProgressChangeCallbackInterval { get; } = TimeSpan.FromSeconds(0.2);

        public static CopyFromArguments Default => new CopyFromArguments();

        private CopyFromArguments()
        {
            
        }
    }
}