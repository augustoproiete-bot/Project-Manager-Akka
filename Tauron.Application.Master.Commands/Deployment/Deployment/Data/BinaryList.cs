using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Data
{
    public sealed class BinaryList : InternalSerializableBase, IReadOnlyList<AppBinary>
    {
        private ImmutableList<AppBinary> _appBinaries = ImmutableList<AppBinary>.Empty;

        public BinaryList(IEnumerable<AppBinary> binaries) 
            => _appBinaries = _appBinaries.AddRange(binaries);

        public BinaryList(BinaryReader reader)
            : base(reader) { }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            _appBinaries = BinaryHelper.Read(reader, r => new AppBinary(r));
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteList(_appBinaries, writer);
            base.WriteInternal(writer);
        }

        public IEnumerator<AppBinary> GetEnumerator() => _appBinaries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _appBinaries).GetEnumerator();

        public int Count => _appBinaries.Count;

        public AppBinary this[int index] => _appBinaries[index];
    }
}