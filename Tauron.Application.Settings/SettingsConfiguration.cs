using Autofac;

namespace Tauron.Application.Settings
{
    public sealed class SettingsConfiguration
    {
        private readonly ContainerBuilder _builder;

        public SettingsConfiguration(ContainerBuilder builder) => _builder = builder;

        public SettingsConfiguration WithProvider<TType>()
            where TType : ISettingProviderConfiguration
        {
            _builder.RegisterType<TType>().As<ISettingProviderConfiguration>();
            return this;
        }
    }
}