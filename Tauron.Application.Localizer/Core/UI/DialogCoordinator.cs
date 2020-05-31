using System;
using System.Threading.Tasks;
using System.Windows;
using Syncfusion.SfSkinManager;
using Tauron.Application.Localizer.UIModels.Views;

namespace Tauron.Application.Localizer.Core.UI
{
    public sealed class DialogCoordinator : IDialogCoordinator
    {
        public static event Action<object>? ShowDialogEvent;

        public static event Action? HideDialogEvent;

        public Task<bool?> ShowMessage(string title, string message, Action<bool?>? result)
        {
            var resultTask = new TaskCompletionSource<bool?>();
            result = result.Combine(b =>
            {
                HideDialog();
                resultTask.SetResult(b);
            });

            ShowDialog(new MessageDialog(title, message, result, true));

            return resultTask.Task;
        }

        public void ShowMessage(string title, string message) => ShowDialog(new MessageDialog(title, message, b => HideDialog(), false));

        public void ShowDialog(object dialog) => ShowDialogEvent?.Invoke(dialog);

        public void HideDialog() => HideDialogEvent?.Invoke();

        public bool? ShowModalMessageWindow(string title, string message)
        {
            var window = new Window();
            SfSkinManager.SetVisualStyle(window, VisualStyles.Blend);
            window.Content = new MessageDialog(title, message, b => window.DialogResult = b, true) { Margin = new Thickness(10)};

            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.WindowStyle = WindowStyle.ToolWindow;
            window.ShowInTaskbar = false;

            return window.ShowDialog();
        }
    }
}