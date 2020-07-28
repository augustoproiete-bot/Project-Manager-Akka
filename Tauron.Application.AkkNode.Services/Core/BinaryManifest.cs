using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Tauron.Application.AkkNode.Services.Core
{
    public sealed class BinaryManifest
    {
        public static void VerifiyEmpty<T>(BinaryReader reader) 
            => Read(reader).Verify(typeof(T).Name, 0);

        public static void WriteEmpty<T>(BinaryWriter writer)
            => Write(writer, typeof(T).Name, 1);

        public static void Write(BinaryWriter writer, string name, int version)
        {
            writer.Write(name);
            writer.Write(version);
        }

        public static BinaryManifest Read(BinaryReader reader)
            => new BinaryManifest(reader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BinaryManifest Read(BinaryReader reader, string name, int minVersion)
        {
            var manifest = Read(reader);
            manifest.Verify(name, minVersion);
            return manifest;
        }

        private readonly string _name;
        private readonly int _version;
        private readonly BinaryReader? _reader;

        private BinaryManifest(BinaryReader reader)
        {
            _name = reader.ReadString();
            _version = reader.ReadInt32();
            _reader = reader;
        }

        public void Verify(string name, int minVersion)
        {
            if(name == _name && _version >= minVersion)
                return;

            throw new NotSupportedException("Verfication Failed");
        }

        public bool WhenVersion(int version)
        {
            if(_reader == null)
                throw new NotSupportedException("No Reader Set");

            return version <= _version;
        }
    }
}