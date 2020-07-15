using System.Threading.Tasks;
using Akka.Actor;

namespace ServiceHost.Installer.Impl.Source
{
    public interface IInstallationSource
    {
        Status ValidateInput(InstallerContext context);

        Task<Status> PreperforCopy(InstallerContext context);
    }
}