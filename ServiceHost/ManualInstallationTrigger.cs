using Akka.Actor;
using Microsoft.Extensions.Configuration;
using ServiceHost.Installer;
using Tauron.Application.AkkaNode.Boottrap;

namespace ServiceHost
{
    public sealed class ManualInstallationTrigger : IStartUpAction
    {
        private readonly IInstaller _installer;
        private readonly ManualInstallationConfiguration _trigger = new ManualInstallationConfiguration();

        public ManualInstallationTrigger(IConfiguration config, IInstaller installer)
        {
            _installer = installer;
            config.Bind(_trigger);
        }

        public void Run()
        {
            if(_trigger.Install != InstallType.Manual) return;

            _installer.Actor.Tell(new FileInstallationRequest(_trigger.AppName, _trigger.ZipFile, _trigger.Override));
        }
    }
}