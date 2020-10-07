using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Akka.Actor;
using Akka.Serialization;
using JetBrains.Annotations;

namespace Tauron.Application.AkkNode.Services.Core
{
    [PublicAPI]
    public static class BinaryHelper
    {
        public static ImmutableDictionary<TKey, TValue> List<TKey, TValue>(BinaryReader reader, Func<BinaryReader, TKey> keyConversion, Func<BinaryReader, TValue> valueConversion)
        {
            var count = reader.ReadInt32();
            var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();

            for (var i = 0; i < count; i++)
                builder.Add(keyConversion(reader), valueConversion(reader));

            return builder.ToImmutable();
        }

        public static void WriteDic(ImmutableDictionary<string, string> dic, BinaryWriter writer)
        {
            writer.Write(dic.Count);
            foreach (var (key, value) in dic)
            {
                writer.Write(key);
                writer.Write(value);
            }
        }

        public static void WriteDic<TKey>(ImmutableDictionary<TKey, string> dic, ActorBinaryWriter writer)
            where TKey : IInternalSerializable
        {
            writer.Write(dic.Count);
            foreach (var (key, value) in dic)
            {
                key.Write(writer);
                writer.Write(value);
            }
        }

        public static ImmutableList<TType> Read<TType>(BinaryReader reader, Func<BinaryReader, TType> converter)
        {
            var builder = ImmutableList.CreateBuilder<TType>();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++) builder.Add(converter(reader));

            return builder.ToImmutable();
        }

        public static ImmutableList<string> ReadString(BinaryReader reader)
        {
            return Read(reader, binaryReader => binaryReader.ReadString());
        }

        public static void WriteList<TType>(ImmutableList<TType> list, ActorBinaryWriter writer)
            where TType : IInternalSerializable
        {
            writer.Write(list.Count);
            foreach (var writeable in list)
                writeable.Write(writer);
        }

        public static void WriteList(ImmutableList<string> list, BinaryWriter writer)
        {
            writer.Write(list.Count);
            foreach (var writeable in list)
                writer.Write(writeable);
        }

        public static void WriteBuffer(byte[] buffer, BinaryWriter writer)
        {
            writer.Write(buffer.Length);
            writer.Write(buffer);
        }

        public static byte[] Readbuffer(BinaryReader reader) 
            => reader.ReadBytes(reader.ReadInt32());

        public static void WriteNull<TValue>([AllowNull] TValue value, BinaryWriter writer, Action<TValue> action)
        {
            if(Equals(value, null))
                writer.Write(false);
            else
            {
                writer.Write(true);
                action(value);
            }
        }

        [return:MaybeNull]
        public static TType ReadNull<TType>(BinaryReader reader, Func<BinaryReader, TType> builder) 
            => reader.ReadBoolean() ? builder(reader) : default;

        public static IActorRef ReadRef(BinaryReader reader, ExtendedActorSystem system)
            => system.Provider.ResolveActorRef(reader.ReadString());


        public static void WriteRef(ActorBinaryWriter writer, IActorRef actor) 
            => writer.Write(Serialization.SerializedActorPath(actor));
    }
}