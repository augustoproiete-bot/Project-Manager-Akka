using System;
using System.Threading.Tasks;
using Tauron.Application.Localizer.UIModels.Views;

namespace Tauron.Application.Localizer.Core.UI
{
    public sealed class DialogCoordinator : IDialogCoordinator
    {
        public static event Action<object> ShowDialogEvent;

        public static event Action HideDialogEvent;

        public Task<bool?> ShowMessage(string title, string message, Action<bool?>? result)
        {
            throw new NotImplementedException();
        }

        public void ShowMessage(string title, string message)
        {
            throw new NotImplementedException();
        }

        public void ShowDialog(object dialog)
        {
            throw new NotImplementedException();
        }

        public void HideDialog()
        {
            throw new NotImplementedException();
        }

        public bool ShowModalMessageWindow(string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}