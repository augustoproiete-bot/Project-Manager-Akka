using System;
using System.Threading.Tasks;
using Tauron.Application.Wpf.Dialogs;

namespace Tauron.Application.ServiceManager.ViewModels.Dialogs
{
    public interface IAddSeedUrlDialog : IBaseDialog<DialogSeedEntry, DialogSeedEntry>
    {
        Task<TResult> MakeTask<TResult>(Func<TaskCompletionSource<TResult>, object> factory);
    }
}