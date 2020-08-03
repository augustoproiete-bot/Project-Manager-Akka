using System;
using System.IO;
using Newtonsoft.Json;
using Tauron.Application.AkkNode.Services.Core;

namespace ExpressionEvaluatorTestService.Api
{
    public sealed class EvaluationResult : InternalSerializableBase
    {
        public object Result { get; private set; }

        public bool Primitive { get; private set; }

        public bool Error { get; private set; }

        public EvaluationResult(object result, bool primitive)
        {
            Result = result;
            Primitive = primitive;
        }

        public EvaluationResult(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Primitive);
            writer.Write(JsonConvert.SerializeObject(Result));
            writer.Write(Result.GetType().AssemblyQualifiedName ?? string.Empty);
            base.WriteInternal(writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
            {
                Primitive = reader.ReadBoolean();
                Result = JsonConvert.DeserializeObject(reader.ReadString(), Type.GetType(reader.ReadString()) ?? typeof(object));
            }

            base.ReadInternal(reader, manifest);
        }
    }
}