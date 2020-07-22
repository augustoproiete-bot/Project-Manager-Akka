using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Tauron.Application.AkkaNode.Boottrap;

namespace ServiceHost.AutoUpdate
{
    public sealed class CleanUpDedector : IStartUpAction
    {
        private readonly IAutoUpdater _updater;
        private readonly bool _switch;

        public CleanUpDedector(IConfiguration configuration, IAutoUpdater updater)
        {
            _updater = updater;
            _switch = configuration.GetValue("cleanup", false);
        }

        public void Run()
        {
            if(!_switch) return;

            _updater.Actor.Tell(new StartCleanUp());
        }
    }
}