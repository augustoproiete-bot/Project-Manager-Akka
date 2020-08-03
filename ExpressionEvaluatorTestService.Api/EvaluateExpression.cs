using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace ExpressionEvaluatorTestService.Api
{
    public sealed class EvaluateExpression : InternalSerializableBase
    {
        public string Data { get; private set; }

        public bool Script { get; private set; }

        public EvaluateExpression(string data, bool script)
        {
            Data = data;
            Script = script;
        }

        public EvaluateExpression(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Data);
            writer.Write(Script);
            base.WriteInternal(writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
            {
                Data = reader.ReadString();
                Script = reader.ReadBoolean();
            }
            base.ReadInternal(reader, manifest);
        }
    }
}