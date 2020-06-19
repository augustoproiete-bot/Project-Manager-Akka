using System;
using System.Threading.Tasks;

namespace Tauron.Application.Wpf.Dialogs
{
    public interface IDialogCoordinator
    {
        public event Action<System.Windows.Window>? OnWindowConstructed;

        Task<bool?> ShowMessage(string title, string message, Action<bool?>? result);

        void ShowMessage(string title, string message);
        void ShowDialog(object dialog);
        void HideDialog();
        bool? ShowModalMessageWindow(string title, string message);
    }
}