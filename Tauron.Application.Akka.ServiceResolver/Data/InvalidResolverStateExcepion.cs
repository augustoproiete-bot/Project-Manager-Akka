using System;
using System.Runtime.Serialization;

namespace Tauron.Application.Akka.ServiceResolver.Data
{
    [Serializable]
    public class InvalidResolverStateExcepion : Exception
    {
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