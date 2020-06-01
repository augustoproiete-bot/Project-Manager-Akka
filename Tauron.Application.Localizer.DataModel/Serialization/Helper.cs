using System;
using System.Collections.Immutable;
using System.IO;

namespace Tauron.Application.Localizer.DataModel.Serialization
{
    public static class Helper
    {
        public static ImmutableList<TType> Read<TType>(BinaryReader reader, Func<BinaryReader, TType> converter)
        {
            var builder = ImmutableList.CreateBuilder<TType>();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++) builder.Add(converter(reader));

            return builder.ToImmutable();
        }

        public static void WriteList<TType>(ImmutableList<TType> list, BinaryWriter writer)
            where TType : IWriteable
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
    }
}