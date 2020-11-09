using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.ServiceManager.Core.Managment.Events;
using Tauron.Application.Workshop.Mutating.Changes;
using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    public class ServicesConfigurationData : IStateEntity, ICanApplyChange<ServicesConfigurationData>
    {
        bool IStateEntity.IsDeleted => false;

        string IStateEntity.Id => nameof(ServicesConfigurationData);

        private readonly AppConfig _appConfig;

        public ServicesConfigurationData(AppConfig appConfig) => _appConfig = appConfig;

        public string BaseConfiguration => _appConfig.CurrentConfig;

        public ServicesConfigurationData Apply(MutatingChange apply)
        {
            switch (apply)
            {
                case ConfigurationChangedEvent configurationChanged:
                    _appConfig.CurrentConfig = configurationChanged.NewConfig;
                    return this;
                default:
                    return this;
            }
        }
    }
}