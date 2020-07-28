using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.Serialization;

namespace Tauron.Application.AkkNode.Services.Core
{
    public sealed class InternalSerializer : Serializer
    {
        private static ConcurrentDictionary<Type, Func<BinaryReader, ExtendedActorSystem, object>> _factorys = new ConcurrentDictionary<Type, Func<BinaryReader, ExtendedActorSystem, object>>();

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
            Func<BinaryReader, ExtendedActorSystem, object> GetFactory()
            {
                if (_factorys.TryGetValue(type, out var fac))
                    return fac;

                foreach (var constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var param = constructor.GetParameterTypes().ToArray();

                    if(param.Length == 0)
                        continue;
                    if (param.Length == 1 && param[0] == typeof(BinaryReader))
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

                _factorys[type] = fac ?? throw new NotSupportedException("No Constrzctor Found");
                return fac;
            }

            using var mem = new MemoryStream(bytes);
            using var reader = new BinaryReader(mem);
            return GetFactory()(reader, system);
        }

        public override bool IncludeManifest { get; } = false;
    }

    public interface IInternalSerializable
    {
        void Write(BinaryWriter writer);
    }
}