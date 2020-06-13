using Tauron.Application.SoftwareRepo.Data;
using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.SoftwareRepo.Mutation
{
    public class CommonChange : MutatingChange
    {
        public ApplicationList ApplicationList { get; }

        public CommonChange(ApplicationList applicationList) => ApplicationList = applicationList;
    }
}