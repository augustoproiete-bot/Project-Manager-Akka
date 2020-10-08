using System;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public sealed class OperationResult : InternalSerializableBase
    {
        public static OperationResult Success(InternalSerializableBase? result = null) => new OperationResult(true, null, result);

        public static OperationResult Failure(string error) => new OperationResult(false, error, null);

        public static OperationResult Failure(Exception error) => new OperationResult(false, error.Unwrap().Message, null);

        public bool Ok { get; private set; }

        public string? Error { get; private set; }
        
        public object? Outcome { get; private set; }

        public OperationResult(BinaryReader reader, ExtendedActorSystem system)
            : base(reader)
        { }

        public OperationResult(bool ok, string? error, InternalSerializableBase? outcome)
        {
            Ok = ok;
            Error = error;
            Outcome = outcome;
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            writer.Write(Ok);
            BinaryHelper.WriteNull(Error, writer, writer.Write);
            BinaryHelper.WriteNull(Outcome, writer, o =>
            {
                writer.Write(o!.GetType().AssemblyQualifiedName);
                BinaryHelper.WriteBuffer(writer.System.Serialization.FindSerializerFor(o).ToBinary(o), writer);
            });

            base.WriteInternal(writer);
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest, ExtendedActorSystem system)
        {
            Ok = reader.ReadBoolean();
            Error = BinaryHelper.ReadNull(reader, r => r.ReadString());
            Outcome = BinaryHelper.ReadNull(reader, r =>
            {
                var type = Type.GetType(reader.ReadString());
                return system.Serialization.FindSerializerForType(type).FromBinary(BinaryHelper.Readbuffer(r), type);
            });
            
            base.ReadInternal(reader, manifest);
        }
    }
}