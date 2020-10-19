using System;
using System.Runtime.Serialization;

namespace Tauron.Application.AkkNode.Services.Commands
{
    [Serializable]
    public class CommandFailedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CommandFailedException()
        {
        }

        public CommandFailedException(string message) : base(message)
        {
        }

        public CommandFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CommandFailedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}