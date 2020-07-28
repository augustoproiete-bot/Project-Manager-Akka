using System.IO;
using Akka.Actor;

namespace Tauron.Application.AkkNode.Services.Core
{
    public sealed class ActorBinaryWriter : BinaryWriter
    {
        public ExtendedActorSystem System { get; }

        public ActorBinaryWriter(Stream stream, ExtendedActorSystem system)  
            : base(stream)
        {
            System = system;
        }
    }
}