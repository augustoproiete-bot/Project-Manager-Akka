using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Deployment.Data
{
    public sealed class AppList : InternalSerializableBase, IReadOnlyCollection<AppInfo>
    {
        private ImmutableList<AppInfo> _app = ImmutableList<AppInfo>.Empty;

        public AppList(IEnumerable<AppInfo> app) 
            => _app = _app.AddRange(app);

        public AppList(BinaryReader reader)
            : base(reader)
        {
            
        }

        protected override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            _app = BinaryHelper.Read(reader, binaryReader => new AppInfo(binaryReader));
            base.ReadInternal(reader, manifest);
        }

        protected override void WriteInternal(ActorBinaryWriter writer)
        {
            BinaryHelper.WriteList(_app, writer);
            base.WriteInternal(writer);
        }

        public IEnumerator<AppInfo> GetEnumerator() => _app.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _app).GetEnumerator();

        public int Count => _app.Count;
    }
}