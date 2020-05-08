namespace Tauron.Host
{
    public sealed class HostEnviroment : IHostEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
    }
}