using ServiceManagerIpProbe.Phase;
using ServiceManagerIpProbe.Phases;

namespace ServiceManagerIpProbe
{
    public static class Operation
    {
        private static PhaseManager<OperationContext> CreateManager() 
            => new PhaseManager<OperationContext>(
                new SetConfigAndConnectPhase(),
                new TryGetDataPhase(),
                new ExtractAndInstallPhase(), 
                new SelfDestroyPhase());

        public static void Start(OperationContext context) 
            => CreateManager().RunNext(context);
    }
}