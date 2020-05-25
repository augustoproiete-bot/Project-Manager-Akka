namespace Tauron.Application.Settings
{
    public interface ISettingProviderConfiguration
    {
        string Scope { get; }

        ISettingProvider Provider { get; }
    }
}