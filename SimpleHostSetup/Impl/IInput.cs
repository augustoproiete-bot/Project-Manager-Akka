using System.Collections.Generic;

namespace SimpleHostSetup.Impl
{
    public interface IInput
    {
        string GetIp();

        string GetName(string @for);

        IEnumerable<string> GetAppsToInstall(IEnumerable<string> apps);
    }
}