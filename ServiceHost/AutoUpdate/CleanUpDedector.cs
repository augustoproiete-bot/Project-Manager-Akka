using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Tauron.Application.AkkaNode.Bootstrap;

namespace ServiceHost.AutoUpdate
{
    public sealed class CleanUpDedector : IStartUpAction
    {
        private readonly IAutoUpdater _updater;
        private readonly bool _switch;
        private readonly int _id;

        public CleanUpDedector(IConfiguration configuration, IAutoUpdater updater)
        {
            _updater = updater;
            _switch = configuration.GetValue("cleanup", false);
            _id = configuration.GetValue("id", -1);
        }

        public void Run()
        {
            if(!_switch) return;

            _updater.Actor.Tell(new StartCleanUp(_id));
        }
    }
}