using System.IO;
using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Installer
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

            if (_trigger.AppType == AppType.Host)
                _trigger.AppName = "Host Self Update";

            _installer.Actor.Tell(new FileInstallationRequest(_trigger.AppName, Path.GetFullPath(_trigger.ZipFile), _trigger.Override, _trigger.AppType, _trigger.Exe));
        }
    }
}