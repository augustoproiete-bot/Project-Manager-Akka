using Amadevus.RecordGenerator;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    [Record]
    public sealed partial class MutatingContext
    {
        public MutatingChange? Change { get; }

        public TType GetChange<TType>()
            where TType : MutatingChange
            => (TType) Change!;

        public ProjectFile File { get; }
    }
}