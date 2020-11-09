using System.Threading.Tasks;
using Tauron.Application.ServiceManager.Core.Configuration;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    [DataSource]
    public sealed class ServicesConfigurationDataSource : SingleValueDataFactory<ServicesConfigurationData>
    {
        private readonly AppConfig _appConfig;

        public ServicesConfigurationDataSource(AppConfig appConfig) => _appConfig = appConfig;

        protected override Task<ServicesConfigurationData> CreateValue() 
            => Task.FromResult(new ServicesConfigurationData(_appConfig));
    }
}