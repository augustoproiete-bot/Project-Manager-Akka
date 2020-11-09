using JetBrains.Annotations;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.ServiceManager.Core.Managment.States;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.Core.Model
{
    [Processor]
    public class ServicesConfigProcessor : ActorModel
    {
        private readonly DeploymentServices _deploymentServices;

        public ServicesConfigProcessor(IActionInvoker actionInvoker, DeploymentServices deploymentServices) 
            : base(actionInvoker)
        {
            _deploymentServices = deploymentServices;

            WhenStateChanges<ServicesConfigState>().FromEvent(s => s.ConfigChanged)
               .ToFlow().Action(NewConfiguration);
        }

        private void NewConfiguration(ConfigurationChangedEvent obj)
        {
            Log.Info("Sending New Configuration to Deloyment Service");
            _deploymentServices.PushNewConfigString(obj.NewConfig);
        }
    }
}