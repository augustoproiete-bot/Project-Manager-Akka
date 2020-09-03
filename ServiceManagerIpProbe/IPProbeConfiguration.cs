namespace ServiceManagerIpProbe
{
    public sealed class IpProbeConfiguration
    {
        public const string DefaultFileName = "probeConfig.json";

        public string Identifer { get; }

        public string TargetAdress { get; }

        public IpProbeConfiguration(string identifer, string targetAdress)
        {
            Identifer = identifer;
            TargetAdress = targetAdress;
        }
    }
}