using System;
using System.Collections.Immutable;
using System.IO;
using Functional.Maybe;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Serialization
{
    public static class BinaryHelper
    {
        public static Maybe<Unit> Write(Maybe<BinaryWriter> mayWriter, string str)
        {
            return
                from writer in mayWriter
                select Action(() => writer.Write(str));
        }

        public static Maybe<string> ReadString(Maybe<BinaryReader> mayReader)
        {
            return
                from reader in mayReader
                select reader.ReadString();
        }

        public static Maybe<Unit> Write(Maybe<BinaryWriter> mayWriter, bool value)
        {
            return
                from writer in mayWriter
                select Action(() => writer.Write(value));
        }

        public static Maybe<bool> ReadBoolean(Maybe<BinaryReader> mayReader)
        {
            return
                from reader in mayReader
                select reader.ReadBoolean();
        }

        public static Maybe<Unit> Write(Maybe<BinaryWriter> mayWriter, int value)
        {
            return
                from writer in mayWriter
                select Action(() => writer.Write(value));
        }

        public static Maybe<int> ReadInt32(Maybe<BinaryReader> mayReader)
        {
            return
                from reader in mayReader
                select reader.ReadInt32();
        }

        public static Maybe<ImmutableDictionary<TKey, TValue>> ReadDic<TKey, TValue>(Maybe<BinaryReader> mayReader, Func<Maybe<BinaryReader>, Maybe<TKey>> keyConversion, Func<Maybe<BinaryReader>, Maybe<TValue>> valueConversion)
            where TKey : notnull
        {
            return
                from reader in mayReader
                let count = reader.ReadInt32()
                let builder = ImmutableDictionary.CreateBuilder<TKey, TValue>()
                from _ in Func(() =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        var pair =
                            from key in keyConversion(mayReader)
                            from value in valueConversion(mayReader)
                            select (key, value);

                        if (pair.IsNothing())
                            return Maybe<Unit>.Nothing;

                        pair.Do(p => builder.Add(p.key, p.value));
                    }

                    return Unit.MayInstance;
                })
                select builder.ToImmutable();
        }
        
        public static Maybe<Unit> WriteDic(Maybe<BinaryWriter> mayWriter, ImmutableDictionary<string, string> dic)
        {
            return
                from writer in mayWriter
                select Action(() =>
                {

                    writer.Write(dic.Count);
                    foreach (var (key, value) in dic)
                    {
                        writer.Write(key);
                        writer.Write(value);
                    }
                });
        }

        public static Maybe<Unit> WriteDic<TKey>(Maybe<BinaryWriter> mayWriter, ImmutableDictionary<TKey, string> dic)
            where TKey : IWriteable
        {
            return
                from writer in mayWriter
                select Action(() =>
                {
                    writer.Write(dic.Count);
                    foreach (var (key, value) in dic)
                    {
                        key.WriteData(mayWriter);
                        writer.Write(value);
                    }
                });
        }

        public static Maybe<ImmutableList<TType>> ReadList<TType>(Maybe<BinaryReader> mayReader, Func<Maybe<BinaryReader>, Maybe<TType>> converter)
        {
            return
                from reader in mayReader
                let builder = ImmutableList.CreateBuilder<TType>()
                let count = reader.ReadInt32()
                from _ in Func(() =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        var data =
                            from value in converter(mayReader)
                            select value;

                        if (data.IsNothing())
                            return Maybe<Unit>.Nothing;

                        Do(data, builder.Add);
                    }

                    return Unit.MayInstance;
                })
                select builder.ToImmutable();
        }

        public static Maybe<ImmutableList<string>> ReadList(Maybe<BinaryReader> reader) 
            => ReadList(reader, binaryReader => from stringReader in binaryReader
                                            select stringReader.ReadString());

        public static Maybe<Unit> WriteList<TType>(Maybe<BinaryWriter> mayWriter, ImmutableList<TType> list)
            where TType : IWriteable
        {
            return
                from writer in mayWriter
                select Action(() =>
                {
                    writer.Write(list.Count);
                    foreach (var writeable in list)
                        writeable.WriteData(mayWriter);
                });
        }

        public static Maybe<Unit> WriteList(Maybe<BinaryWriter> mayWriter, ImmutableList<string> list)
        {
            return
                from writer in mayWriter
                select Action(() =>
                {
                    writer.Write(list.Count);
                    foreach (var writeable in list)
                        writer.Write(writeable);
                });
        }
    }
}