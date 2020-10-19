using System;
using System.Runtime.Serialization;

namespace Tauron.Application.AkkNode.Services.FileTransfer.TemporarySource
{
    [Serializable]
    public class TempFileInUseException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TempFileInUseException()
        {
        }

        public TempFileInUseException(string message) : base(message)
        {
        }

        public TempFileInUseException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TempFileInUseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}