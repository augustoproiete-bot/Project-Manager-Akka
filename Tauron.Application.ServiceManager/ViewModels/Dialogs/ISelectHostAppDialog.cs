using System.Threading.Tasks;
using Tauron.Application.Master.Commands.Host;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.ServiceManager.ViewModels.Dialogs
{
    public interface ISelectHostAppDialog : IBaseDialog<HostApp?, Task<HostApp[]>>
    {
        
    }
}