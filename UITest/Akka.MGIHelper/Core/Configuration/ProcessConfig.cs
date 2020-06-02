using Akka.MGIHelper.Settings;
using Tauron.Akka;

namespace Akka.MGIHelper.Core.Configuration
{
    public class ProcessConfig : ConfigurationBase
    {
        public ProcessConfig(IDefaultActorRef<SettingsManager> actor, string scope)
            : base(actor, scope)
        {
        }

        public string Kernel => GetValue(s => s, string.Empty)!;

        public string Client => GetValue(s => s, string.Empty)!;
    }
}