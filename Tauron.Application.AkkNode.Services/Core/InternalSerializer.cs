using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.Serialization;

namespace Tauron.Application.AkkNode.Services.Core
{
    public sealed class InternalSerializer : SerializerWithStringManifest
    {
        private static readonly ConcurrentDictionary<Type, Func<BinaryReader, ExtendedActorSystem, object>> Factorys = new ConcurrentDictionary<Type, Func<BinaryReader, ExtendedActorSystem, object>>();

        public InternalSerializer(ExtendedActorSystem system) : base(system)
        {
        }

        public override int Identifier { get; } = 100;

        public override byte[] ToBinary(object obj)
        {
            if (obj is IInternalSerializable serializable)
            {
                using var mem = new MemoryStream();
                using var writer = new ActorBinaryWriter(mem, system);
                serializable.Write(writer);
                writer.Flush();
                mem.Position = 0;
                return mem.ToArray();
            }

            throw new NotSupportedException("no IInternalSerializable");
        }


        public override object FromBinary(byte[] bytes, string manifest)
        {
            var type = Type.GetType(manifest, true);

            Func<BinaryReader, ExtendedActorSystem, object> GetFactory()
            {
                if (Factorys.TryGetValue(type, out var fac))
                    return fac;

                foreach (var constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var param = constructor.GetParameterTypes().ToArray();

                    if (param.Length == 0)
                        continue;
                    if (param.Length == 1 && param[0].IsAssignableFrom(typeof(BinaryReader)))
                    {
                        fac = (binaryReader, actorSystem) => constructor.FastCreate(binaryReader);
                        break;
                    }

                    if (param.Length != 2) continue;

                    if (param[0].IsAssignableFrom(typeof(BinaryReader)) && param[1].IsAssignableFrom(typeof(ExtendedActorSystem)))
                    {
                        fac = (binaryReader, actorSystem) => constructor.FastCreate(binaryReader, actorSystem);
                        break;
                    }
                    if (!param[0].IsAssignableFrom(typeof(ExtendedActorSystem)) || !param[1].IsAssignableFrom(typeof(BinaryReader))) continue;
                    {
                        fac = (binaryReader, actorSystem) => constructor.FastCreate(actorSystem, binaryReader);
                        break;
                    }
                }

                Factorys[type] = fac ?? throw new NotSupportedException("No Constrzctor Found");
                return fac;
            }

            using var mem = new MemoryStream(bytes);
            using var reader = new BinaryReader(mem);
            return GetFactory()(reader, system);
        }

        public override string Manifest(object o) => o.GetType().AssemblyQualifiedName;
    }
}