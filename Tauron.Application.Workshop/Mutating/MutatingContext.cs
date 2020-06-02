using Amadevus.RecordGenerator;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.Workshop.Mutating
{
    [Record]
    public sealed partial class MutatingContext<TData>
    {
        public MutatingChange? Change { get; }

        public TData Data { get; }

        public TType GetChange<TType>()
            where TType : MutatingChange
        {
            return (TType) Change!;
        }
    }
}