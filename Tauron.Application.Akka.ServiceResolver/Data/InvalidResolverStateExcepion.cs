using System;
using System.Runtime.Serialization;

namespace Tauron.Application.Akka.ServiceResolver.Data
{
    [Serializable]
    public class InvalidResolverStateExcepion : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidResolverStateExcepion()
        {
        }

        public InvalidResolverStateExcepion(string message) : base(message)
        {
        }

        public InvalidResolverStateExcepion(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidResolverStateExcepion(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}