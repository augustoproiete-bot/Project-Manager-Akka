using ServiceManagerIpProbe.Phase;
using ServiceManagerIpProbe.Phases;

namespace ServiceManagerIpProbe
{
    public static class Operation
    {
        public const string Identifer = nameof(Identifer);

        public const string Deny = nameof(Deny);

        public const string Accept = nameof(Accept);

        public const string Data = nameof(Data);

        public const string Compled = nameof(Compled);

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