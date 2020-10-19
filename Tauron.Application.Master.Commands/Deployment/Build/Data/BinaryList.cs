using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    public sealed class BinaryList : IEnumerable<AppBinary>
    {
        public ImmutableList<AppBinary> AppBinaries { get; }

        public BinaryList(ImmutableList<AppBinary> appBinaries) => AppBinaries = appBinaries;
        public IEnumerator<AppBinary> GetEnumerator() => AppBinaries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) AppBinaries).GetEnumerator();
    }
}