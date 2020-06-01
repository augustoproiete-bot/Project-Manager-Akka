using System;
using System.Globalization;
using System.IO;
using Amadevus.RecordGenerator;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    public sealed partial class ActiveLanguage : IEquatable<ActiveLanguage>, IWriteable
    {
        public static readonly ActiveLanguage Invariant = FromCulture(CultureInfo.InvariantCulture);

        public string Shortcut { get; }

        public string Name { get; }

        public ActiveLanguage(BinaryReader reader)
        {
            Shortcut = reader.ReadString();
            Name = reader.ReadString();
        }

        public static ActiveLanguage FromCulture(CultureInfo info)
            => new ActiveLanguage(info.Name, info.EnglishName);

        public CultureInfo ToCulture() => CultureInfo.GetCultureInfo(Shortcut);

        public bool Equals(ActiveLanguage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Shortcut == other.Shortcut;
        }

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ActiveLanguage other && Equals(other);

        public override int GetHashCode() => Shortcut.GetHashCode();

        public static bool operator ==(ActiveLanguage? left, ActiveLanguage? right) => Equals(left, right);

        public static bool operator !=(ActiveLanguage? left, ActiveLanguage? right) => !Equals(left, right);

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shortcut);
            writer.Write(Name);
        }

        public static ActiveLanguage ReadFrom(BinaryReader reader)
        {
            
        }
    }
}