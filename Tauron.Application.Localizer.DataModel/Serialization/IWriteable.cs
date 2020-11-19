using System.IO;
using Functional.Maybe;

namespace Tauron.Application.Localizer.DataModel.Serialization
{
    public interface IWriteable
    {
        Maybe<Unit> Write(Maybe<BinaryWriter> mayWriter);
    }
}