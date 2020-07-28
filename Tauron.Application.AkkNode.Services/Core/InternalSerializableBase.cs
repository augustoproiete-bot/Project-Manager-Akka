using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Core
{
    [PublicAPI]
    public abstract class InternalSerializableBase : IInternalSerializable
    {
        protected virtual int Version => 1;

        protected InternalSerializableBase()
        {
            
        }

        protected InternalSerializableBase(BinaryReader reader)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            var manifest = BinaryManifest.Read(reader, GetType().Name, Version);

            // ReSharper disable once VirtualMemberCallInConstructor
            ReadInternal(reader, manifest);
        }

        protected virtual void ReadInternal(BinaryReader reader, BinaryManifest manifest) { }

        public void Write(ActorBinaryWriter writer)
        {
            BinaryManifest.Write(writer, GetType().Name, Version);
            WriteInternal(writer);
        }

        protected virtual void WriteInternal(ActorBinaryWriter writer) { }
    }
}