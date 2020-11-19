using System.Globalization;
using System.IO;
using Functional.Maybe;
using Tauron.Application.Localizer.DataModel.Serialization;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel
{
    public sealed record ActiveLanguage(string Shortcut, string Name) : IWriteable
    {
        public static readonly Maybe<ActiveLanguage> Invariant = FromCulture(May(CultureInfo.InvariantCulture));

        public static Maybe<ActiveLanguage> FromCulture(Maybe<CultureInfo> mayInfo)
            => from info in mayInfo
               select new ActiveLanguage(info.Name, info.EnglishName);

        public Maybe<CultureInfo> ToCulture() 
            => May(CultureInfo.GetCultureInfo(Shortcut));

        public static Maybe<ActiveLanguage> ReadFrom(Maybe<BinaryReader> mayReader)
            => from reader in mayReader
               select new ActiveLanguage(reader.ReadString(), reader.ReadString());

        public Maybe<Unit> Write(Maybe<BinaryWriter> mayWriter)
            => from writer in mayWriter
               select Action(() =>
                             {
                                 writer.Write(Shortcut);
                                 writer.Write(Name);
                             });
    }
}