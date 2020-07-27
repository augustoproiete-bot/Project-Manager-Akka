using System;
using System.IO;
using Akka.Actor;
using Akka.Serialization;

namespace Tauron.Application.AkkNode.Services.Core
{
    public sealed class InternalSerializer : Serializer
    {
        public InternalSerializer(ExtendedActorSystem system) 
            : base(system)
        {
            
        }

        public override byte[] ToBinary(object obj)
        {
            if (obj is IInternalSerializable serializable)
            {
                using var mem = new MemoryStream();
                using var writer = new BinaryWriter(mem);
                serializable.Write(writer);
                mem.Position = 0;
                return mem.ToArray();
            }

            throw new NotSupportedException("no IInternalSerializable");
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            using var mem = new MemoryStream(bytes);
            using var reader = new BinaryReader(mem);
            return Activator.CreateInstance(type, reader, system);
        }

        public override bool IncludeManifest { get; } = false;
    }

    public interface IInternalSerializable
    {
        void Write(BinaryWriter writer);
    }
}