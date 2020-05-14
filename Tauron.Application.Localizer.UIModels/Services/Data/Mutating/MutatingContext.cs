using Amadevus.RecordGenerator;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.UIModels.Services.Data.Mutating.Changes;

namespace Tauron.Application.Localizer.UIModels.Services.Data.Mutating
{
    [Record]
    public sealed partial class MutatingContext
    {
        public MutatingChange? Change { get; }

        public ProjectFile File { get; }
    }
}