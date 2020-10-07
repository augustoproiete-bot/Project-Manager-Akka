using System.Collections.Immutable;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Data
{
    public sealed class BinaryList : InternalSerializableBase
    {
        private ImmutableList<AppBinary> _appBinaries = ImmutableList<AppBinary>.Empty;sdg
    }
}