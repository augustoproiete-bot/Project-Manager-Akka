using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.AkkNode.Services.CleanUp
{
    public sealed class StartCleanUp : InternalSerializableBase
    {
        public StartCleanUp()
        {
            
        }

        public StartCleanUp(BinaryReader reader)
            : base(reader)
        {
            
        }
    }
}