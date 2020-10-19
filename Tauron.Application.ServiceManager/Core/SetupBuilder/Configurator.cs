using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Akka.Configuration;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class Configurator
    {
        private readonly string _settingsPath;

        private readonly StringBuilder _config = new StringBuilder();
        private readonly Config _baseConfig = Config.Empty;

        public Configurator(string appPath)
        {
            _settingsPath = Path.Combine(appPath, "seed.conf");

            if (File.Exists(_settingsPath))
                _baseConfig = ConfigurationFactory.ParseString(File.ReadAllText(_settingsPath));
        }

        public void SetIp(string ip) 
            => _config.AppendLine($"akka.remote.dot-netty.tcp.hostname = \"{ip}\"");

        public void SetSeed(IEnumerable<string> ips) 
            => _config.AppendLine($"akka.cluster.seed-nodes = [{string.Join(',', ips.Select(s => $"\"{s}\""))}]");

        public void SetAppName(string name)
            => _config.AppendLine($"akka.appinfo.applicationName: \"{name}\"");

        public void Save()
        {
            if(_config.Length == 0)
                return;

            var newConfig = ConfigurationFactory.ParseString(_config.ToString());

            newConfig = newConfig.WithFallback(_baseConfig);

            File.WriteAllText(_settingsPath, newConfig.ToString(true));
        }
    }
}