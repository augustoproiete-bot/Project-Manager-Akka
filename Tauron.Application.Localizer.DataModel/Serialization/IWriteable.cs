using System.IO;

namespace Tauron.Application.Localizer.DataModel.Serialization
{
    public interface IWriteable
    {
        void Write(BinaryWriter writer);
    }
}