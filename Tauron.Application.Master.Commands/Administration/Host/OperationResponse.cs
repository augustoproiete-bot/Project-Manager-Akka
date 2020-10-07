using System.IO;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed class OperationResponse : InternalSerializableBase
    {
        public bool Success { get; private set; }
        
        public OperationResponse(bool success) 
            => Success = success;

        [UsedImplicitly]
        public OperationResponse(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Success);
            base.WriteInternal(writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            if (manifest.WhenVersion(1))
                Success = reader.ReadBoolean();
            base.ReadInternal(reader, manifest);
        }
    }
}