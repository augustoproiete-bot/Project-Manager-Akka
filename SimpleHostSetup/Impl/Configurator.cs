using System.Text;
using Akka.Configuration;

namespace SimpleHostSetup.Impl
{
    public sealed class Configurator
    {
        private readonly string _appPath;
        p

        private readonly StringBuilder _config = new StringBuilder();
        private Config _baseConfig = Config.Empty;

        public Configurator(string appPath)
        {
            _appPath = appPath;
        }

        public void SetIp(string ip)
        {

        }
    }
}