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

        public bool Equals(ActiveLanguage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Shortcut == other.Shortcut;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Shortcut);
            writer.Write(Name);
        }

        public static ActiveLanguage FromCulture(CultureInfo info)
        {
            return new ActiveLanguage(info.Name, info.EnglishName);
        }

        public CultureInfo ToCulture()
        {
            return CultureInfo.GetCultureInfo(Shortcut);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ActiveLanguage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Shortcut.GetHashCode();
        }

        public static bool operator ==(ActiveLanguage? left, ActiveLanguage? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActiveLanguage? left, ActiveLanguage? right)
        {
            return !Equals(left, right);
        }

        public static ActiveLanguage ReadFrom(BinaryReader reader)
        {
            var lang = new Builder
            {
                Shortcut = reader.ReadString(),
                Name = reader.ReadString()
            };
            return lang.ToImmutable();
        }
    }
}