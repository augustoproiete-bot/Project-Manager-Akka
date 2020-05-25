using System;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using Optional;

namespace Tauron
{
    /// <summary>
    /// A static class for basic stream operations.
    /// </summary>
    [PublicAPI]
    public static class StreamHelper
    {
        /// <summary>
        /// Copies the source stream into the current while reporting the progress.
        /// The copying process is done in a separate thread, therefore the stream has to 
        /// support reading from a different thread as the one used for construction.
        /// Nethertheless, the separate thread is synchronized with the calling thread.
        /// The callback in arguments is called from the calling thread.
        /// </summary>
        /// <param name="targetOption">The current stream</param>
        /// <param name="sourceOption">The source stream</param>
        /// <param name="argumentsOption">The arguments for copying</param>
        /// <returns>The number of bytes actually copied.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either target, source of arguments is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arguments.BufferSize is less than 128 or arguments.ProgressChangeCallbackInterval is less than 0</exception>
        public static Option<long> CopyFrom(this Option<Stream> targetOption, Option<Stream> sourceOption, Option<CopyFromArguments> argumentsOption = default)
        {
            return targetOption
               .FlatMap(o => (o, sourceOption.ValueOr(Stream.Null), argumentsOption.ValueOr(CopyFromArguments.Default)).Some())
               .FlatMap(o =>
                        {
                            (Stream source, Stream target, CopyFromArguments arguments) = o;

                            // ReSharper disable NotResolvedInText
                            if(arguments.BufferSize < 128)
                            {
                                throw new ArgumentOutOfRangeException("arguments.BufferSize",
                                    arguments.BufferSize, "BufferSize has to be greater or equal than 128.");
                            }
                            if(arguments.ProgressChangeCallbackInterval.TotalSeconds < 0)
                            {
                                throw new ArgumentOutOfRangeException("arguments.ProgressChangeCallbackInterval",
                                    arguments.ProgressChangeCallbackInterval,
                                    "ProgressChangeCallbackInterval has to be greater or equal than 0.");
                            }

                            // ReSharper restore NotResolvedInText

                            long length = 0;
                            var bufferSize = arguments.BufferSize;
                            byte[] buffer = new byte[bufferSize];

                            var runningFlag = true;

                            Action<Stream, Stream> copyMemory = (targetParm, sourceParm) =>
                                                                         //Raw copy-operation, "length" and "runningFlag" are enclosed as closure
                                                                     {
                                                                         int count;

                                                                         // ReSharper disable AccessToModifiedClosure
                                                                         while ((count = sourceParm.Read(buffer, 0, bufferSize)) != 0 && runningFlag)
                                                                         {
                                                                             targetParm.Write(buffer, 0, count);
                                                                             var newLength = length + count;
                                                                             //"length" can be read as this is the only thread which writes to "length"
                                                                             Interlocked.Exchange(ref length, newLength);
                                                                         }
                                                                         // ReSharper restore AccessToModifiedClosure
                                                                     };

                            IAsyncResult asyncResult = copyMemory.BeginInvoke(target, source, null, null);

                            var totalLength = arguments.TotalLength;
                            if (totalLength == -1 && source.CanSeek) totalLength = source.Length;

                            var lastCallback = DateTime.Now;
                            long lastLength = 0;

                            while (!asyncResult.IsCompleted)
                            {
                                if (arguments.StopEvent != null && arguments.StopEvent.WaitOne(0))
                                    runningFlag = false; //to indicate that the copy-operation has to abort

                                Thread.Sleep((int) (arguments.ProgressChangeCallbackInterval.TotalMilliseconds / 10));

                                if (arguments.ProgressChangeCallback == null || DateTime.Now - lastCallback <= arguments.ProgressChangeCallbackInterval) continue;

                                var currentLength = Interlocked.Read(ref length); //Since length is 64 bit, reading is not an atomic operation.

                                if (currentLength == lastLength) continue;

                                lastLength = currentLength;
                                lastCallback = DateTime.Now;
                                arguments.ProgressChangeCallback(currentLength, totalLength);
                            }

                            if (arguments.ProgressChangeCallback != null && lastLength != length)
                                //to ensure that the callback is called once with maximum progress
                                arguments.ProgressChangeCallback(length, totalLength);

                            copyMemory.EndInvoke(asyncResult);

                            return length.Some();
                        });
        }

        /// <summary>
        /// Copies the source stream into the current
        /// </summary>
        /// <param name="streamOption">The current stream</param>
        /// <param name="sourceOption">The source stream</param>
        /// <param name="bufferSize">The size of buffer used for copying bytes</param>
        /// <returns>The number of bytes actually copied.</returns>
        public static Option<long> CopyFrom(this Option<Stream> streamOption, Option<Stream> sourceOption, int bufferSize = 4096)
        {
            return streamOption
               .FlatMap(o => (o, sourceOption.ValueOr(Stream.Null)).Some())
               .FlatMap(o =>
                        {
                            (Stream source, Stream stream) = o;

                            int count;
                            byte[] buffer = new byte[bufferSize];
                            long length = 0;

                            while ((count = source.Read(buffer, 0, bufferSize)) != 0)
                            {
                                length += count;
                                stream.Write(buffer, 0, count);
                            }

                            return length.Some();
                        });
        }
    }

}
