using System.Globalization;
using System.IO;
using Functional.Maybe;
using Tauron.Application.Localizer.DataModel.Serialization;
using static Tauron.Prelude;
using static Tauron.Application.Localizer.DataModel.Serialization.BinaryHelper;

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
            => from shortcut in ReadString(mayReader)
                from name in ReadString(mayReader)
                select new ActiveLanguage(shortcut, name);

        public Maybe<Unit> WriteData(Maybe<BinaryWriter> mayWriter)
            => from _ in Write(mayWriter, Shortcut)
                from u in Write(mayWriter, Name)
                select u;
    }
}