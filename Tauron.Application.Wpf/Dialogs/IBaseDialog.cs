using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tauron.Application.Wpf.Dialogs
{
    public interface IBaseDialog<TData, in TViewData>
    {
        Task<TData> Init(IEnumerable<TViewData> initalData);
    }
}