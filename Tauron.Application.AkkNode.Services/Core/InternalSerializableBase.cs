using System.IO;
using Akka.Actor;
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

        protected InternalSerializableBase(BinaryReader reader, ExtendedActorSystem? system = null)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            var manifest = BinaryManifest.Read(reader, GetType().Name, Version);

            if(system == null)
                // ReSharper disable once VirtualMemberCallInConstructor
                ReadInternal(reader, manifest);
            else
                // ReSharper disable once VirtualMemberCallInConstructor
                ReadInternal(reader, manifest, system);
        }

        protected virtual void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system) { ReadInternal(reader, manifest); }

        protected virtual void ReadInternal(BinaryReader reader, BinaryManifest manifest){}

        public void Write(ActorBinaryWriter writer)
        {
            BinaryManifest.Write(writer, GetType().Name, Version);
            WriteInternal(writer);
        }

        protected virtual void WriteInternal(ActorBinaryWriter writer){}
    }
}