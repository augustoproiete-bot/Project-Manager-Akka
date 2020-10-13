using System.Threading.Tasks;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.ServiceManager.ViewModels.Dialogs
{
    public interface ISelectHostAppDialog : IBaseDialog<HostApp?, Task<HostApp[]>>
    {
        
    }
}