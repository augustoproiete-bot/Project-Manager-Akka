﻿using System;
using System.Runtime.Serialization;

namespace Tauron.Application.Akka.ServiceResolver
{
    [Serializable]
    public class ResolverException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ResolverException()
        {
        }

        public ResolverException(string message) : base(message)
        {
        }

        public ResolverException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ResolverException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}